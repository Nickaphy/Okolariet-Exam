using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;

namespace Web.Templates
{
    // The content of one slot: text, a media file and/or (for buttons) the page it leads to.
    public sealed record SlotContent(string? Text, Guid? AssetId = null, Guid? TargetId = null);

    // One page, as the template components see it. The kiosk (published snapshot) and the editor (live data)
    // both turn their own API types into this, so the templates don't depend on where the data came from.
    public sealed record PageContent(
        Guid Id,
        string TemplateType,
        string? Title,
        Appearance? Appearance,
        IReadOnlyDictionary<string, SlotContent> Slots)
    {
        public static PageContent From(PublishedModdedTemplate p) => new(
            p.Id, p.TemplateType, p.Title, p.Appearance,
            Map(p.Items?.Select(i => (i.SlotKey, new SlotContent(i.Text, i.AssetId, i.TargetModdedTemplateId)))));

        public static PageContent From(ModdedTemplate m) => new(
            m.Id, m.TemplateType, m.Title, m.Appearance,
            Map(m.Items?.Select(i => (i.SlotKey, new SlotContent(i.Text, i.AssetId, i.TargetModdedTemplateId)))));

        // An empty page of a given type (shows only placeholders in a preview).
        public static PageContent Empty(string templateType) =>
            new(Guid.Empty, templateType, null, null, new Dictionary<string, SlotContent>());

        private static IReadOnlyDictionary<string, SlotContent> Map(IEnumerable<(string Key, SlotContent Content)>? items)
        {
            var result = new Dictionary<string, SlotContent>();
            foreach (var (key, content) in items ?? Enumerable.Empty<(string, SlotContent)>())
            {
                if (!string.IsNullOrEmpty(key)) result[key] = content;   // last one wins, never throws on duplicates
            }
            return result;
        }
    }

    // What every slot component inside a template gets (as a cascading value from TemplateHost).
    public sealed class TemplateScope(
        PageContent page,
        Template? definition,
        bool preview,
        bool placeholders,
        string assetBaseUrl,
        EventCallback<Guid> navigate)
    {
        public PageContent Page { get; } = page;

        // The template definition from the server, if known. Only used for placeholder labels ("Knap 1").
        public Template? Definition { get; } = definition;

        // Preview = nothing can be tapped (editor and thumbnails).
        public bool Preview { get; } = preview;

        // Placeholders = empty slots are drawn as dashed boxes, so staff can see where things go.
        public bool Placeholders { get; } = placeholders;

        public EventCallback<Guid> Navigate { get; } = navigate;

        public SlotContent? Slot(string key) => Page.Slots.GetValueOrDefault(key);

        public bool HasText(string key) => !string.IsNullOrWhiteSpace(Slot(key)?.Text);

        public string LabelOf(string key) =>
            Definition?.Slots?.FirstOrDefault(s => s.Key == key)?.Label ?? key;

        public string AssetUrl(Guid id) => $"{assetBaseUrl}/assets/{id}";
    }
}
