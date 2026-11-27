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
    //
    // The rules of an exhibition live here and nowhere else:
    // - the main menu comes first (no pages before it exists);
    // - pages are only created, linked and removed through this class, so the "next" links stay in one piece;
    // - a page built from a Menu template is a sub-menu and ends its path.
    // Nothing else is required of the staff: a project with only a main menu is fine.
    public sealed class ExhibitionEditor(
        IProjectsApi projects,
        IModdedTemplatesApi pages,
        IItemsApi items,
        IVersionsApi versions)
    {
        // ---- project ----

        public Task<string?> RenameProjectAsync(ExhibitionModel m, string name) =>
            Guard(() => SaveProjectAsync(m, name, m.Project.MainModdedTemplateId, m.Project.Appearance));

        // Colours and font for the whole exhibition. null = nothing chosen, so the neutral default is used.
        public Task<string?> SaveAppearanceAsync(ExhibitionModel m, Appearance? appearance) =>
            Guard(() => SaveProjectAsync(m, m.Project.Name, m.Project.MainModdedTemplateId, appearance));

        // Every exhibition has a main menu. It is created from one of the fixed menu templates.
        public Task<string?> CreateMenuAsync(ExhibitionModel m, string menuTemplateType) => Guard(async () =>
        {
            var created = await pages.CreateModdedTemplate(m.Project.Id,
                new ModdedTemplateCreate { TemplateType = menuTemplateType, Title = "Hovedmenu", SortOrder = 0 });
            if (Fail(created) is { } error) return error;

            return await SaveProjectAsync(m, m.Project.Name, created.Content!.Id, m.Project.Appearance);
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

        // ---- texts on a page ----

        // Writes one text slot (e.g. the main menu's heading). An empty text clears the slot.
        public Task<string?> SaveTextAsync(Guid pageId, string slotKey, string text) => Guard(async () =>
        {
            var value = text.Trim();
            if (value.Length == 0)
                return Fail(await items.ClearItem(pageId, slotKey), ignoreNotFound: true);

            return Fail(await items.SetItem(pageId, slotKey, new ModdedTemplateItemInput { Text = value }));
        });

        // ---- menu buttons and their paths ----

        // Works for the buttons of the main menu and of sub-menus: the button knows which menu it sits on.
        public Task<string?> SaveButtonLabelAsync(ButtonPath button, string label) => Guard(() =>
        {
            var firstPage = button.Pages.Count > 0 ? button.Pages[0].Page.Id : (Guid?)null;
            return WriteButtonAsync(button.OwnerId, button.Slot.Key, label.Trim(), firstPage);
        });

        // Creates a page from a fixed template and puts it last in the button's path.
        // Any template can be used, also a menu template (that page then becomes a sub-menu).
        public Task<string?> AddPageAsync(ExhibitionModel m, ButtonPath button, string templateType) => Guard(async () =>
        {
            if (m.Menu is null) return "Opret hovedmenuen, før du tilføjer sider.";
            if (button.EndsInMenu)
                return "Forløbet slutter i en undermenu. Tilføj siden på en af undermenuens knapper i stedet.";

            var created = await pages.CreateModdedTemplate(m.Project.Id, new ModdedTemplateCreate
            {
                TemplateType = templateType,
                Title = TitleFor(m, templateType),
                SortOrder = m.Pages.Count
            });
            if (Fail(created) is { } error) return error;

            var newId = created.Content!.Id;
            var linkError = button.Pages.Count == 0
                ? await WriteButtonAsync(button.OwnerId, button.Slot.Key, button.Label, newId)
                : await WriteNextAsync(button.Pages[^1].Page.Id, newId);

            return linkError is null
                ? null
                : $"Siden blev oprettet, men kunne ikke sættes ind i forløbet (den står under «Sider uden knap»). {linkError}";
        });

        // Takes a page out of a path (the pages around it are joined up) and deletes it.
        // A sub-menu takes the pages below it with it, so nothing is left behind unreachable.
        public Task<string?> RemovePageAsync(ButtonPath button, int index) => Guard(async () =>
        {
            var node = button.Pages[index];
            Guid? next = index + 1 < button.Pages.Count ? button.Pages[index + 1].Page.Id : null;

            var relinkError = index == 0
                ? await WriteButtonAsync(button.OwnerId, button.Slot.Key, button.Label, next)
                : await WriteNextAsync(button.Pages[index - 1].Page.Id, next);
            if (relinkError is not null) return relinkError;

            // Parents before children: the server refuses to delete a page that another page still links to.
            foreach (var page in node.WithSubPages())
            {
                if (Fail(await pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true) is { } error)
                    return error;
            }
            return null;
        });

        public Task<string?> DeletePageAsync(ModdedTemplate page) => Guard(async () =>
            Fail(await pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true));

        // ---- helpers ----

        // New pages are named after their template ("Faktaside", then "Faktaside 2" ...). Staff can rename them later.
        private static string TitleFor(ExhibitionModel m, string templateType)
        {
            var name = m.DisplayNameOf(templateType);
            var sameType = m.Pages.Count(p => p.TemplateType == templateType);
            var title = sameType == 0 ? name : $"{name} {sameType + 1}";
            return title.Length > 120 ? title[..120] : title;
        }

        private async Task<string?> SaveProjectAsync(ExhibitionModel m, string name, Guid? mainMenuId, Appearance? appearance) // Save a project
        {
            if (m.ETag is null) return "Siden skal hentes igen, før ændringen kan gemmes. Genindlæs siden.";

            var response = await projects.UpdateProject(m.Project.Id, new ProjectInput
            {
                Name = name,
                MainModdedTemplateId = mainMenuId,
                Appearance = appearance
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