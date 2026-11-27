using System.Net;
using Web.Api;
using Web.Api.Contracts;
using Web.Exceptions;
using Web.Ports.Items;
using Web.Ports.ModdedTemplates;
using Web.Ports.Projects;
using Web.Ports.Versions;

namespace Web.Services.Editor
{
    public sealed record PublishResult(int? Version, string? Error);

    // Every change the editor page can make to an exhibition.
    //
    // Every method returns null when it worked, or a Danish error message if it failed.
    // That is what the dialogs expect, so the page can hand these methods straight to a dialog.
    // After a change the page reloads the whole ExhibitionModel, so this class keeps no state of its own.
    //
    // The rules of an exhibition live here and nowhere else:
    // - the main menu comes first (no pages can be added before it exists);
    // - pages are only created, linked and removed through this class, so the "next" links (PathConvention) stay whole;
    // - a page built from a Menu template is a sub-menu and ends its path.
    // Nothing else is required: a project with only a main menu is fine.
    //
    // How the error handling works: the gateways throw when the API says no. ApiCall.Try around a whole method
    // catches that and returns the message, so the code inside can simply be written as if every call works.
    // Where a step needs its own message (e.g. "the page was created, but could not be linked"), ApiCall.Run is used.
    public sealed class ExhibitionEditor(
        IProjectGateway projects,
        IModdedTemplateGateway pages,
        IItemGateway items,
        IVersionGateway versions)
    {
        // ---- the project itself ----

        public Task<string?> RenameProjectAsync(ExhibitionModel m, string name) =>
            ApiCall.Try(() => SaveProjectAsync(m, name, m.Project.MainModdedTemplateId, m.Project.Appearance));

        // Colours and font for the whole exhibition. null = nothing chosen, so the neutral default is used.
        public Task<string?> SaveAppearanceAsync(ExhibitionModel m, Appearance? appearance) =>
            ApiCall.Try(() => SaveProjectAsync(m, m.Project.Name, m.Project.MainModdedTemplateId, appearance));

        // Creates the main menu from one of the fixed menu templates, and makes it the project's start page.
        public Task<string?> CreateMenuAsync(ExhibitionModel m, string menuTemplateType) => ApiCall.Try(async () =>
        {
            var menu = await pages.CreateModdedTemplate(m.Project.Id,
                new ModdedTemplateCreate { TemplateType = menuTemplateType, Title = "Hovedmenu", SortOrder = 0 });

            return await SaveProjectAsync(m, m.Project.Name, menu.Id, m.Project.Appearance);
        });

        // Publishing returns the new version number too, so it has its own small result type.
        public async Task<PublishResult> PublishAsync(Guid projectId)
        {
            var (version, error) = await ApiCall.Get(() => versions.PublishVersion(projectId),
                (HttpStatusCode.UnprocessableEntity, "Udstillingen er ikke færdig endnu. Udfyld det, der mangler på siderne, og prøv igen."));
            return new PublishResult(version?.VersionNumber, error);
        }

        // ---- texts on a page ----

        // Writes one text slot (e.g. the main menu's heading). An empty text clears the slot.
        public Task<string?> SaveTextAsync(Guid pageId, string slotKey, string text) =>
            ApiCall.Try(async () =>
            {
                await WriteSlotAsync(pageId, slotKey, text.Trim(), target: null);
                return null;
            });

        // ---- menu buttons and their paths ----

        // Works for buttons on the main menu and on sub-menus: the button knows which menu it sits on (OwnerId).
        // The button keeps pointing at the first page of its path.
        public Task<string?> SaveButtonLabelAsync(ButtonPath button, string label) =>
            ApiCall.Try(async () =>
            {
                await WriteSlotAsync(button.OwnerId, button.Slot.Key, label.Trim(), button.FirstPageId);
                return null;
            });

        // Creates a page from a fixed template and puts it last in the button's path.
        // Any template can be used, also a menu template (the page then becomes a sub-menu).
        public Task<string?> AddPageAsync(ExhibitionModel m, ButtonPath button, string templateType) => ApiCall.Try(async () =>
        {
            if (m.Menu is null) return "Opret hovedmenuen, før du tilføjer sider.";
            if (button.EndsInMenu)
                return "Forløbet slutter i en undermenu. Tilføj siden på en af undermenuens knapper i stedet.";

            var page = await pages.CreateModdedTemplate(m.Project.Id, new ModdedTemplateCreate
            {
                TemplateType = templateType,
                Title = TitleFor(m, templateType),
                SortOrder = m.Pages.Count
            });

            // Link it in: an empty path → the button points at it. Otherwise → the last page's "next" points at it.
            var linkError = await ApiCall.Run(() => button.Pages.Count == 0
                ? WriteSlotAsync(button.OwnerId, button.Slot.Key, button.Label, page.Id)
                : WriteNextAsync(button.Pages[^1].Page.Id, page.Id));

            return linkError is null
                ? null
                : $"Siden blev oprettet, men kunne ikke sættes ind i forløbet (den står under «Sider uden knap»). {linkError}";
        });

        // Takes page number "index" out of a path, joins the pages on each side of it, and deletes it.
        // A sub-menu takes all pages below it along, so nothing is left behind that no one can reach.
        public Task<string?> RemovePageAsync(ButtonPath button, int index) => ApiCall.Try(async () =>
        {
            var removed = button.Pages[index];
            Guid? after = index + 1 < button.Pages.Count ? button.Pages[index + 1].Page.Id : null;

            // 1. Whatever pointed at the removed page now points at the page after it (or at nothing).
            if (index == 0) await WriteSlotAsync(button.OwnerId, button.Slot.Key, button.Label, after);
            else await WriteNextAsync(button.Pages[index - 1].Page.Id, after);

            // 2. Delete. Parents before children: the server refuses to delete a page another page still links to.
            foreach (var page in removed.WithSubPages())
            {
                var error = await ApiCall.Run(() => pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true);
                if (error is not null) return error;
            }
            return null;
        });

        // Deletes a page that no button leads to (shown under "Sider uden knap").
        public Task<string?> DeletePageAsync(ModdedTemplate page) =>
            ApiCall.Run(() => pages.DeleteModdedTemplate(page.Id), ignoreNotFound: true);

        // ---- helpers ----

        // New pages are named after their template ("Faktaside", then "Faktaside 2" ...). Max 120 characters (API rule).
        private static string TitleFor(ExhibitionModel m, string templateType)
        {
            var name = m.DisplayNameOf(templateType);
            var sameType = m.Pages.Count(p => p.TemplateType == templateType);
            var title = sameType == 0 ? name : $"{name} {sameType + 1}";
            return title.Length > 120 ? title[..120] : title;
        }

        // PUT /projects/{id} replaces name, main menu and appearance together, so all three are always sent.
        // The ETag (If-Match) makes the server refuse the save if someone else changed the project meanwhile.
        private async Task<string?> SaveProjectAsync(ExhibitionModel m, string name, Guid? mainMenuId, Appearance? appearance)
        {
            if (m.ETag is null) return "Siden skal hentes igen, før ændringen kan gemmes. Genindlæs siden.";

            await projects.UpdateProject(m.Project.Id, new ProjectInput
            {
                Name = name,
                MainModdedTemplateId = mainMenuId,
                Appearance = appearance
            }, m.ETag);
            return null;
        }

        // One slot item holds both a text and (for buttons) the page it leads to, so they are written together.
        // Nothing left in it → the item is removed instead (already gone is fine).
        private async Task WriteSlotAsync(Guid pageId, string slotKey, string text, Guid? target)
        {
            if (text.Length == 0 && target is null)
            {
                try
                {
                    await items.ClearItem(pageId, slotKey);
                }
                catch (GatewayException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    // The slot was already empty. That is the result we wanted.
                }
                return;
            }

            await items.SetItem(pageId, slotKey, new ModdedTemplateItemInput
            {
                Text = text.Length == 0 ? null : text,
                TargetModdedTemplateId = target
            });
        }

        // The hidden "next" slot of a page: which page comes after it in the path.
        private Task WriteNextAsync(Guid pageId, Guid? next) =>
            WriteSlotAsync(pageId, PathConvention.NextSlotKey, text: string.Empty, next);
    }
}
