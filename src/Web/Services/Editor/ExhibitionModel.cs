using Web.Api.Contracts;

namespace Web.Services.Editor
{

    // How templates that are meant to come after eacher other are stored. This is necessary because the Database doesn't store the base templates, and we thus we can't just "save the order".
    // - a main-menu button slot points (targetModdedTemplateId) at the FIRST page of its path;
    // - every page points at the NEXT page through a slot called "next".

    // If in future development, the templates need to be stored in the database (and this problem becomes void), this file and ExhibitionEditor are the only places to change.
    public static class PathConvention
    {
        public const string NextSlotKey = "next";
    }

    // One button on the main menu and the pages it leads through, in order.
    public sealed record ButtonPath(SlotDefinition Slot, string Label, IReadOnlyList<ModdedTemplate> Pages);

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
