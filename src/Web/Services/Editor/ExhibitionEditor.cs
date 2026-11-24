using System.Net;
using Web.Api;
using Web.Api.Contracts;
using Refit;

namespace Web.Services.Editor
{

    public sealed record PublishResult(int? Version, string? Error);

    // Everything the editor can change. Every method returns null on success or a error message,
    // which is exactly what NameDialog.Submit and ConfirmDialog.Run expect, so they plug straight in.
    // After a change the page reloads the model, so nothing here has to keep state.
    public sealed class ExhibitionEditor(
        IProjectsApi projects,
        IModdedTemplatesApi pages,
        IItemsApi items,
        IVersionsApi versions)
    {
        // ---- project ----

        public Task<string?> RenameProjectAsync(ExhibitionModel m, string name) =>
            Guard(() => SaveProjectAsync(m, name, m.Project.MainModdedTemplateId));

        // Every exhibition has a main menu. It is created from one of the fixed menu templates.
        public Task<string?> CreateMenuAsync(ExhibitionModel m, string menuTemplateType) => Guard(async () =>
        {
            var created = await pages.CreateModdedTemplate(m.Project.Id,
                new ModdedTemplateCreate { TemplateType = menuTemplateType, Title = "Hovedmenu", SortOrder = 0 });
            if (Fail(created) is { } error) return error;

            return await SaveProjectAsync(m, m.Project.Name, created.Content!.Id);
        });

        public async Task<PublishResult> PublishAsync(Guid projectId)
        {
            try
            {
                var response = await versions.PublishVersion(projectId);
                if (response.IsSuccessStatusCode) return new PublishResult(response.Content?.VersionNumber, null);

                return new PublishResult(null, response.StatusCode == HttpStatusCode.UnprocessableEntity
                    ? "Udstillingen er ikke færdig endnu. Udfyld det, der mangler på siderne, og prøv igen."
                    : ApiErrors.Describe(response));
            }
            catch (HttpRequestException)
            {
                return new PublishResult(null, ApiErrors.Offline);
            }
        }

        // ---- menu buttons and their paths ----

        public Task<string?> SaveButtonLabelAsync(ExhibitionModel m, ButtonPath button, string label) => Guard(() =>
        {
            var firstPage = button.Pages.Count > 0 ? button.Pages[0].Id : (Guid?)null;
            return WriteButtonAsync(m.Menu!.Id, button.Slot.Key, label.Trim(), firstPage);
        });

        // Creates a template from a fixed template and puts it last in the button's path.
        public Task<string?> AddPageAsync(ExhibitionModel m, ButtonPath button, string templateType, string title) => Guard(async () =>
        {
            var created = await pages.CreateModdedTemplate(m.Project.Id, new ModdedTemplateCreate
            {
                TemplateType = templateType,
                Title = title,
                SortOrder = m.Pages.Count
            });
            if (Fail(created) is { } error) return error;

            var newId = created.Content!.Id;
            var linkError = button.Pages.Count == 0
                ? await WriteButtonAsync(m.Menu!.Id, button.Slot.Key, button.Label, newId)
                : await WriteNextAsync(button.Pages[^1].Id, newId);

            return linkError is null
                ? null
                : $"Siden blev oprettet, men kunne ikke sættes ind i forløbet (den står under «Sider uden knap»). {linkError}";
        });

        // Takes a template out of a path (the pages around it are joined up) and deletes it.
        public Task<string?> RemovePageAsync(ExhibitionModel m, ButtonPath button, int index) => Guard(async () =>
        {
            var page = button.Pages[index];
            Guid? next = index + 1 < button.Pages.Count ? button.Pages[index + 1].Id : null;

            var relinkError = index == 0
                ? await WriteButtonAsync(m.Menu!.Id, button.Slot.Key, button.Label, next)
                : await WriteNextAsync(button.Pages[index - 1].Id, next);
            if (relinkError is not null) return relinkError;

            return Fail(await pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true);
        });

        public Task<string?> DeletePageAsync(ModdedTemplate page) => Guard(async () =>
            Fail(await pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true));

        // ---- helpers ----

        private async Task<string?> SaveProjectAsync(ExhibitionModel m, string name, Guid? mainMenuId) // Save a project
        {
            if (m.ETag is null) return "Siden skal hentes igen, før ændringen kan gemmes. Genindlæs siden.";

            var response = await projects.UpdateProject(m.Project.Id, new ProjectInput
            {
                Name = name,
                MainModdedTemplateId = mainMenuId,
                Appearance = m.Project.Appearance
            }, m.ETag);

            return Fail(response);
        }

        // A button slot holds both the label and where it leads, in one item. Write them together.
        private async Task<string?> WriteButtonAsync(Guid menuId, string slotKey, string label, Guid? target)
        {
            if (label.Length == 0 && target is null)
                return Fail(await items.ClearItem(menuId, slotKey), ignoreNotFound: true);

            return Fail(await items.SetItem(menuId, slotKey, new ModdedTemplateItemInput
            {
                Text = label.Length == 0 ? null : label,
                TargetModdedTemplateId = target
            }));
        }

        private async Task<string?> WriteNextAsync(Guid pageId, Guid? next)
        {
            if (next is null)
                return Fail(await items.ClearItem(pageId, PathConvention.NextSlotKey), ignoreNotFound: true);

            return Fail(await items.SetItem(pageId, PathConvention.NextSlotKey,
                new ModdedTemplateItemInput { TargetModdedTemplateId = next }));
        }

        private static string? Fail(IApiResponse response, bool ignoreNotFound = false) =>
            response.IsSuccessStatusCode || (ignoreNotFound && response.StatusCode == HttpStatusCode.NotFound)
                ? null
                : ApiErrors.Describe(response);

        private static async Task<string?> Guard(Func<Task<string?>> action) // General method for catching exceptions, used to be more DRY compliant
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException)
            {
                return ApiErrors.Offline;
            }
        }
    }
}
