using Web.Api;
using Web.Api.Contracts;
using Web.Ports.ModdedTemplates;
using Web.Ports.Projects;
using Web.Ports.Templates;

namespace Web.Services.Editor
{
    public sealed record LoadResult(ExhibitionModel? Model, string? Error);

    // Loads everything the editor page shows, and works out which pages belong to which menu button.
    // The result is an ExhibitionModel (see ExhibitionModel.cs for the shape and the "next" convention).
    public sealed class ExhibitionLoader(IProjectGateway projects, ITemplateGateway templates, IModdedTemplateGateway pages)
    {
        public async Task<LoadResult> LoadAsync(Guid projectId, CancellationToken ct = default)
        {
            ExhibitionModel? model = null;

            // ApiCall.Try turns a failing gateway call anywhere inside into a Danish message.
            var error = await ApiCall.Try(async () =>
            {
                // 1. Project (with its ETag, needed to save it later), template definitions and the list of pages, all at once.
                var projectTask = projects.GetProjectWithETag(projectId, ct);
                var templatesTask = templates.ListTemplates(ct);
                var summariesTask = pages.ListModdedTemplates(projectId, ct);
                await Task.WhenAll(projectTask, templatesTask, summariesTask);

                // 2. The list only has summaries (no slot contents), so every page is fetched in full.
                //    An exhibition has a handful of pages, so this is fine for an MVP.
                var fullPages = await Task.WhenAll(summariesTask.Result.Select(s => pages.GetModdedTemplate(s.Id, ct)));

                // 3. Put it together.
                var project = projectTask.Result;
                model = Build(project.Value, project.ETag, templatesTask.Result.ToList(), fullPages.OrderBy(p => p.SortOrder).ToList());
                return null;
            });

            return new LoadResult(model, error);
        }

        private static ExhibitionModel Build(Project project, string? etag, List<Template> templates, List<ModdedTemplate> all)
        {
            var byId = all.ToDictionary(p => p.Id);
            var byType = templates.ToDictionary(t => t.Type);

            // Every page we reach from the main menu is added here. What is left over at the end are "orphans".
            var used = new HashSet<Guid>();

            ModdedTemplate? menu = null;
            Template? menuTemplate = null;
            if (project.MainModdedTemplateId is Guid menuId && byId.TryGetValue(menuId, out var found))
            {
                menu = found;
                used.Add(menu.Id);
                byType.TryGetValue(menu.TemplateType, out menuTemplate);
            }

            IReadOnlyList<ButtonPath> buttons = menu is not null && menuTemplate is not null
                ? ButtonsOf(menu, menuTemplate, byId, byType, used)
                : [];

            return new ExhibitionModel
            {
                Project = project,
                ETag = etag,
                Templates = templates,
                Pages = all,
                Menu = menu,
                MenuTemplate = menuTemplate,
                Buttons = buttons,
                Orphans = all.Where(p => !used.Contains(p.Id)).ToList()
            };
        }

        // The buttons of one menu, each with the pages it leads through, in order.
        //
        // For each button slot of the menu template:
        //   follow the button to its first page, then that page's "next" to the following page, and so on.
        //   If one of those pages is itself a menu (a sub-menu), call this same method for it: that gives the
        //   sub-menu's own buttons and paths. A sub-menu ends the path, so we stop following "next" there.
        //
        // "used" makes sure every page is visited once: it stops a loop (A → B → A) and a page two buttons point at.
        private static List<ButtonPath> ButtonsOf(
            ModdedTemplate menu, Template menuTemplate,
            Dictionary<Guid, ModdedTemplate> byId, Dictionary<string, Template> byType, HashSet<Guid> used)
        {
            var result = new List<ButtonPath>();

            foreach (var slot in menuTemplate.Slots.Where(s => s.Kind == SlotDefinitionKind.Button))
            {
                var buttonItem = ItemOf(menu, slot.Key);
                var path = new List<PathPage>();

                var nextId = buttonItem?.TargetModdedTemplateId;
                while (nextId is Guid id && byId.TryGetValue(id, out var page) && used.Add(id))
                {
                    byType.TryGetValue(page.TemplateType, out var pageTemplate);
                    var isMenu = pageTemplate?.Category == TemplateCategory.Menu;

                    IReadOnlyList<ButtonPath> subButtons = isMenu ? ButtonsOf(page, pageTemplate!, byId, byType, used) : [];
                    path.Add(new PathPage(page, pageTemplate, subButtons));

                    nextId = isMenu ? null : ItemOf(page, PathConvention.NextSlotKey)?.TargetModdedTemplateId;
                }

                result.Add(new ButtonPath(menu.Id, slot, buttonItem?.Text ?? string.Empty, path));
            }

            return result;
        }

        private static ModdedTemplateItem? ItemOf(ModdedTemplate page, string slotKey) =>
            page.Items?.FirstOrDefault(i => i.SlotKey == slotKey);
    }
}
