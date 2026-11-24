using Web.Api;
using Web.Api.Contracts;
using Refit;

namespace Web.Services.Editor
{
    public sealed record LoadResult(ExhibitionModel? Model, string? Error);

    // Loads a project with its templates and pages, and works out which pages belong to which menu button. Heavily relies on the ExhibitionModel.cs
    // Talks to Refit directly (not the gateways) because the project's ETag header is needed for saving.
    public sealed class ExhibitionLoader(IProjectsApi projects, ITemplatesApi templates, IModdedTemplatesApi pages)
    {
        public async Task<LoadResult> LoadAsync(Guid projectId, CancellationToken ct = default)
        {
            try
            {
                var projectTask = projects.GetProject(projectId, ct);
                var templatesTask = templates.ListTemplates(ct);
                var summariesTask = pages.ListModdedTemplates(projectId, ct);
                await Task.WhenAll(projectTask, templatesTask, summariesTask);

                var projectResponse = await projectTask;
                var templatesResponse = await templatesTask;
                var summariesResponse = await summariesTask;

                foreach (IApiResponse r in new IApiResponse[] { projectResponse, templatesResponse, summariesResponse })
                {
                    if (!r.IsSuccessStatusCode) return new LoadResult(null, ApiErrors.Describe(r));
                }

                // The summaries have no slot contents, so every page is fetched in full.
                // A museum exhibition has a handful of pages, so this is fine for an MVP.
                var fullPages = await Task.WhenAll(
                    summariesResponse.Content!.Select(s => pages.GetModdedTemplate(s.Id, ct)));

                var failed = fullPages.FirstOrDefault(r => !r.IsSuccessStatusCode);
                if (failed is not null) return new LoadResult(null, ApiErrors.Describe(failed));

                return new LoadResult(Build(
                    projectResponse.Content!,
                    projectResponse.Headers?.ETag?.Tag,
                    templatesResponse.Content!.ToList(),
                    fullPages.Select(r => r.Content!).OrderBy(p => p.SortOrder).ToList()), null);
            }
            catch (HttpRequestException)
            {
                return new LoadResult(null, ApiErrors.Offline);
            }
        }

        private static ExhibitionModel Build(Project project, string? etag, List<Template> templates, List<ModdedTemplate> all)
        {
            var byId = all.ToDictionary(p => p.Id);

            var menu = project.MainModdedTemplateId is { } menuId && byId.TryGetValue(menuId, out var m) ? m : null;
            var menuTemplate = menu is null ? null : templates.FirstOrDefault(t => t.Type == menu.TemplateType);

            var buttons = new List<ButtonPath>();
            var used = new HashSet<Guid>();

            if (menu is not null && menuTemplate is not null)
            {
                foreach (var slot in menuTemplate.Slots.Where(s => s.Kind == SlotDefinitionKind.Button))
                {
                    var item = ItemOf(menu, slot.Key);
                    var path = new List<ModdedTemplate>();

                    // Follow button → first page → next → next ... "used" also stops loops
                    // and a page that two buttons point at.
                    var next = item?.TargetModdedTemplateId;
                    while (next is { } id && byId.TryGetValue(id, out var page) && used.Add(id))
                    {
                        path.Add(page);
                        next = ItemOf(page, PathConvention.NextSlotKey)?.TargetModdedTemplateId;
                    }

                    buttons.Add(new ButtonPath(slot, item?.Text ?? string.Empty, path));
                }
            }

            return new ExhibitionModel
            {
                Project = project,
                ETag = etag,
                Templates = templates,
                Pages = all,
                Menu = menu,
                MenuTemplate = menuTemplate,
                Buttons = buttons,
                Orphans = all.Where(p => p.Id != menu?.Id && !used.Contains(p.Id)).ToList()
            };
        }

        private static ModdedTemplateItem? ItemOf(ModdedTemplate page, string slotKey) =>
            page.Items?.FirstOrDefault(i => i.SlotKey == slotKey);
    }
}
