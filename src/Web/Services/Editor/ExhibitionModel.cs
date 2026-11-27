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

        // Every page below this one (the paths behind a sub-menu's buttons), parents before children.
        // Calls WithSubPages on each of those pages, so it goes all the way down through sub-sub-menus.
        // For a normal page (not a menu) Buttons is empty, so this is empty too.
        public IEnumerable<ModdedTemplate> SubPages() =>
            Buttons.SelectMany(button => button.Pages.SelectMany(page => page.WithSubPages()));

        // This page first, then everything below it. The order deletes can safely happen in.
        public IEnumerable<ModdedTemplate> WithSubPages() => SubPages().Prepend(Page);
    }

    // One button on a menu (the main menu or a sub-menu) and the pages it leads through, in order.
    // OwnerId is the menu the button sits on.
    public sealed record ButtonPath(Guid OwnerId, SlotDefinition Slot, string Label, IReadOnlyList<PathPage> Pages)
    {
        // The page the button itself points at (null when the path is empty).
        public Guid? FirstPageId => Pages.Count > 0 ? Pages[0].Page.Id : null;

        // A path that ends in a sub-menu can't get more pages: the sub-menu's own buttons lead on.
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
            TemplateOf(templateType)?.DisplayName ?? templateType;

        public Template? TemplateOf(string templateType) =>
            Templates.FirstOrDefault(t => t.Type == templateType);

        // Every button in the exhibition, the main menu's and all sub-menus', each with a short description of
        // where it is ("Hovedmenu · Knap 2 «Quiz»"). Used to pick where a page should go.
        public IEnumerable<(ButtonPath Button, string Where)> AllButtons() => ButtonsBelow(Buttons, "Hovedmenu");

        private static IEnumerable<(ButtonPath, string)> ButtonsBelow(IReadOnlyList<ButtonPath> buttons, string menuName)
        {
            foreach (var button in buttons)
            {
                var label = button.Label.Length > 0 ? $" «{button.Label}»" : "";
                yield return (button, $"{menuName} · {button.Slot.Label}{label}");

                // The buttons of a sub-menu at the end of this path, described with the sub-menu's own name.
                foreach (var page in button.Pages.Where(p => p.IsMenu))
                    foreach (var below in ButtonsBelow(page.Buttons, $"Undermenu «{page.Page.Title}»"))
                        yield return below;
            }
        }

        // Where a page sits: which button's path, and its position (0-based) in that path.
        // null for the main menu and for pages no button leads to.
        public (ButtonPath Button, int Index)? FindSpot(Guid pageId)
        {
            foreach (var (button, _) in AllButtons())
            {
                for (var i = 0; i < button.Pages.Count; i++)
                    if (button.Pages[i].Page.Id == pageId) return (button, i);
            }
            return null;
        }
    }
}