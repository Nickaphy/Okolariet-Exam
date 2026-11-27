using Web.Api.Contracts;

namespace Web.Services.Editor
{
    // How pages that come after each other are stored. The database doesn't keep the order of pages, so:
    // - a menu button points (targetModdedTemplateId) at the FIRST page of its path;
    // - every page points at the NEXT page through a slot called "next".
    // If the order is ever stored in the database, this file, the loader and ExhibitionEditor are the only places to change.
    public static class PathConvention
    {
        public const string NextSlotKey = "next";
    }

    // One page in a path. A page built from a Menu template is a sub-menu: it has buttons of its own,
    // each with a path of its own, and it ends the path it sits in (nothing comes "next" after a menu).
    public sealed record PathPage(ModdedTemplate Page, Template? Template, IReadOnlyList<ButtonPath> Buttons)
    {
        public bool IsMenu => Template?.Category == TemplateCategory.Menu;

        // Every page below this one (sub-menu buttons and their paths), parents before children.
        public IEnumerable<ModdedTemplate> SubPages() =>
            Buttons.SelectMany(b => b.Pages.SelectMany(p => p.WithSubPages()));

        public IEnumerable<ModdedTemplate> WithSubPages() => SubPages().Prepend(Page);
    }

    // One button on a menu (the main menu or a sub-menu) and the pages it leads through, in order.
    // OwnerId is the menu the button sits on.
    public sealed record ButtonPath(Guid OwnerId, SlotDefinition Slot, string Label, IReadOnlyList<PathPage> Pages)
    {
        public bool EndsInMenu => Pages.Count > 0 && Pages[^1].IsMenu;

        public bool LastIs(string templateType) => Pages.Count > 0 && Pages[^1].Page.TemplateType == templateType;

        // 1-based page numbers in this path that use the template. Used for "Brugt på side 1".
        public IReadOnlyList<int> PositionsOf(string templateType) => Pages
            .Select((p, i) => (p, number: i + 1))
            .Where(x => x.p.Page.TemplateType == templateType)
            .Select(x => x.number)
            .ToList();
    }

    // Everything the editor page needs, loaded in one go.
    public sealed class ExhibitionModel
    {
        public required Project Project { get; init; }

        // ETag of the project from the last GET. Needed as If-Match when saving the project.
        public required string? ETag { get; init; }

        public required IReadOnlyList<Template> Templates { get; init; }
        public required IReadOnlyList<ModdedTemplate> Pages { get; init; }

        public ModdedTemplate? Menu { get; init; }
        public Template? MenuTemplate { get; init; }
        public IReadOnlyList<ButtonPath> Buttons { get; init; } = [];

        // Pages that no button leads to (e.g. after a half-failed save). Shown so they can be cleaned up.
        public IReadOnlyList<ModdedTemplate> Orphans { get; init; } = [];

        public string DisplayNameOf(string templateType) =>
            Templates.FirstOrDefault(t => t.Type == templateType)?.DisplayName ?? templateType;
    }
}