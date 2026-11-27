using System.Net;
using Web.Api;
using Web.Api.Contracts;
using Web.Exceptions;
using Web.Ports.Answers;
using Web.Ports.Items;
using Web.Ports.ModdedTemplates;
using Web.Ports.Projects;
using Web.Ports.Questions;
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
        IVersionGateway versions,
        IQuestionGateway questions,
        IAnswerGateway answers)
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

            // Link it in at the end of the path.
            var linkError = await ApiCall.Run(() => PointAtAsync(button, button.Pages.Count, page.Id));

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
            await PointAtAsync(button, index, after);

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

        // ---- re-routing: change the order, take a page out, put a page in ----

        // Moves page number "index" one step up (direction -1) or down (+1) in its path.
        public Task<string?> MovePageAsync(ButtonPath button, int index, int direction) => ApiCall.Try(async () =>
        {
            // The two pages that swap places: "upper" and the one right below it.
            var upper = direction < 0 ? index - 1 : index;
            if (upper < 0 || upper + 1 >= button.Pages.Count) return null;   // already first / last

            var a = button.Pages[upper];
            var b = button.Pages[upper + 1];
            if (b.IsMenu) return "En undermenu er altid den sidste side i et forløb, så den kan ikke rykkes op.";
            Guid? afterB = upper + 2 < button.Pages.Count ? button.Pages[upper + 2].Page.Id : null;

            // Before:  … → A → B → (after B)        After:  … → B → A → (after B)
            await PointAtAsync(button, upper, b.Page.Id);
            await WriteNextAsync(b.Page.Id, a.Page.Id);
            await WriteNextAsync(a.Page.Id, afterB);
            return null;
        });

        // Takes a page out of its path without deleting it. The pages around it are joined up, and the page
        // shows under "Sider uden knap", from where it can be put in somewhere else (AttachPageAsync).
        // A sub-menu takes the pages below it along (they stay linked to it).
        public Task<string?> DetachPageAsync(ButtonPath button, int index) => ApiCall.Try(async () =>
        {
            var page = button.Pages[index];
            Guid? after = index + 1 < button.Pages.Count ? button.Pages[index + 1].Page.Id : null;

            await PointAtAsync(button, index, after);
            if (!page.IsMenu) await WriteNextAsync(page.Page.Id, null);
            return null;
        });

        // Puts an existing page (one from "Sider uden knap") last in a button's path.
        public Task<string?> AttachPageAsync(ButtonPath button, ModdedTemplate page) => ApiCall.Try(async () =>
        {
            if (button.EndsInMenu)
                return "Forløbet slutter i en undermenu. Vælg en af undermenuens knapper i stedet.";
            if (button.Pages.Any(p => p.Page.Id == page.Id)) return null;   // already there

            await PointAtAsync(button, button.Pages.Count, page.Id);
            return null;
        });

        // ---- change the template of a page (also the main menu) ----

        // The API doesn't allow changing a page's template type (it decides which fields the page has).
        // So the change is made as: a new page from the new template → copy everything that fits → point
        // whatever led to the old page at the new one → delete the old page. Nothing a guest can reach is lost
        // on the way: if a step fails before the switch, the new page is removed again and the old one is untouched.
        //
        // What is kept: the title and colours, every field whose key also exists in the new template (e.g.
        // button1-4 when a 4-button menu becomes a 6-button menu, including where the buttons lead), and for a
        // quiz the questions and answers (as many as the new template allows). Only templates of the same kind
        // (menu, fact or quiz) can be swapped, so the fields have something in common.
        public Task<string?> ChangeTemplateAsync(ExhibitionModel m, ModdedTemplate old, string newType) => ApiCall.Try(async () =>
        {
            if (newType == old.TemplateType) return null;
            if (m.TemplateOf(newType) is not { } newTemplate) return "Skabelonen findes ikke på serveren.";
            var oldTemplate = m.TemplateOf(old.TemplateType);
            if (oldTemplate is not null && oldTemplate.Category != newTemplate.Category)
                return "En side kan kun skifte til en skabelon af samme slags (menu, fakta eller quiz).";

            // 1. The new page: same title, colours and order as the old one.
            var created = await pages.CreateModdedTemplate(m.Project.Id, new ModdedTemplateCreate
            {
                TemplateType = newType,
                Title = old.Title,
                SortOrder = old.SortOrder,
                Appearance = old.Appearance
            });

            // Removes the new page again when a step below fails, so no half-made copy is left behind.
            async Task<string> Undo(string error)
            {
                await ApiCall.Run(() => pages.DeleteModdedTemplate(created.Id), ignoreNotFound: true);
                return error;
            }

            // 2. Copy the content.
            var copyError = await ApiCall.Run(() => CopyContentAsync(old, oldTemplate, created.Id, newTemplate));
            if (copyError is not null) return await Undo(copyError);

            // 3. Switch: whatever led to the old page now leads to the new one.
            var switchError = m.Menu?.Id == old.Id
                ? await SaveProjectAsync(m, m.Project.Name, created.Id, m.Project.Appearance)                       // the main menu
                : m.FindSpot(old.Id) is { } spot
                    ? await ApiCall.Run(() => PointAtAsync(spot.Button, spot.Index, created.Id))                    // a page in a path
                    : null;                                                                                          // a page no button leads to
            if (switchError is not null) return await Undo(switchError);

            // 4. The old page is no longer used.
            var deleteError = await ApiCall.Run(() => pages.DeleteModdedTemplate(old.Id), ignoreNotFound: true);
            return deleteError is null
                ? null
                : $"Skabelonen er skiftet, men den gamle side kunne ikke slettes (den står under «Sider uden knap»). {deleteError}";
        });

        // Copies the fields (and a quiz's questions) that fit the new template. See ChangeTemplateAsync.
        private async Task CopyContentAsync(ModdedTemplate old, Template? oldTemplate, Guid newPageId, Template newTemplate)
        {
            foreach (var item in old.Items ?? [])
            {
                // Only fields the new template has, with the same kind (a text field can't hold a picture).
                var slot = newTemplate.Slots?.FirstOrDefault(s => s.Key == item.SlotKey);
                var oldSlot = oldTemplate?.Slots?.FirstOrDefault(s => s.Key == item.SlotKey);
                if (slot is null || (oldSlot is not null && oldSlot.Kind != slot.Kind)) continue;

                await items.SetItem(newPageId, item.SlotKey, new ModdedTemplateItemInput
                {
                    Text = Shorten(item.Text, slot.MaxLength),   // e.g. an 8-button menu allows shorter button texts
                    AssetId = item.AssetId,
                    TargetModdedTemplateId = item.TargetModdedTemplateId
                });
            }

            if (newTemplate.QuestionRules is not { } rules) return;

            foreach (var question in (old.Questions ?? []).OrderBy(q => q.SortOrder).Take(rules.MaxQuestions))
            {
                var copy = await questions.CreateQuestion(newPageId,
                    new QuestionInput { Text = question.Text, AssetId = question.AssetId, SortOrder = question.SortOrder });

                foreach (var answer in AnswersToKeep(question, rules.AnswersPerQuestion))
                {
                    await answers.CreateAnswer(copy.Id, new AnswerInput
                    {
                        Text = answer.Text,
                        AssetId = answer.AssetId,
                        IsCorrect = answer.IsCorrect,
                        SortOrder = answer.SortOrder
                    });
                }
            }
        }

        // When the new quiz allows fewer answers per question (e.g. sandt/falsk has 2): keep the right answer(s)
        // first, fill up with the others, and keep them in their original order.
        public static IEnumerable<Answer> AnswersToKeep(Question question, int limit) =>
            (question.Answers ?? [])
                .OrderByDescending(a => a.IsCorrect).ThenBy(a => a.SortOrder)
                .Take(limit)
                .OrderBy(a => a.SortOrder);

        private static string? Shorten(string? text, int? maxLength) =>
            text is not null && maxLength is int max && text.Length > max ? text[..max] : text;

        // ---- helpers ----

        // The link INTO position "position" of a path:
        //   position 0  → the button itself (on its menu) points at the page;
        //   otherwise   → the "next" slot of the page just before it points at the page.
        // target null = point at nothing (the path ends before this position).
        private Task PointAtAsync(ButtonPath button, int position, Guid? target) => position == 0
            ? WriteSlotAsync(button.OwnerId, button.Slot.Key, button.Label, target)
            : WriteNextAsync(button.Pages[position - 1].Page.Id, target);

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
