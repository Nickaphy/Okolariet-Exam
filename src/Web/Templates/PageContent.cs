using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Templates.Quiz;

namespace Web.Templates
{
    // What staff filled into one slot: a text, a media file (AssetId) and/or, for buttons, the page it leads to (TargetId).
    public sealed record SlotContent(string? Text, Guid? AssetId = null, Guid? TargetId = null);

    // One page, the way the template components see it: which template draws it, its colours, and slot key → content.
    //
    // The kiosk has the published snapshot (PublishedModdedTemplate), the editor has the live data (ModdedTemplate).
    // Both are turned into this, so a template component never needs to know where its data came from.
    public sealed record PageContent(
        Guid Id,
        string TemplateType,
        string? Title,
        Appearance? Appearance,
        IReadOnlyDictionary<string, SlotContent> Slots)
    {
        // The questions of a quiz page, in order. Empty for every other kind of page.
        public IReadOnlyList<QuizQuestion> Questions { get; init; } = [];

        // From the published snapshot (kiosk). The answers have no IsCorrect here, on purpose (see QuizAnswer).
        public static PageContent From(PublishedModdedTemplate p) => new(
            p.Id, p.TemplateType, p.Title, p.Appearance,
            ToSlots(p.Items?.Select(i => (i.SlotKey, new SlotContent(i.Text, i.AssetId, i.TargetModdedTemplateId)))))
        {
            Questions = (p.Questions ?? []).Select(q => new QuizQuestion(q.Id, q.Text, q.AssetId,
                (q.Answers ?? []).OrderBy(a => a.SortOrder).Select(a => new QuizAnswer(a.Id, a.Text, a.AssetId, null)).ToList())).ToList()
        };

        // From the editor's live data. Here the right answers are known.
        public static PageContent From(ModdedTemplate m) => new(
            m.Id, m.TemplateType, m.Title, m.Appearance,
            ToSlots(m.Items?.Select(i => (i.SlotKey, new SlotContent(i.Text, i.AssetId, i.TargetModdedTemplateId)))))
        {
            Questions = (m.Questions ?? []).OrderBy(q => q.SortOrder).Select(q => new QuizQuestion(q.Id, q.Text, q.AssetId,
                (q.Answers ?? []).OrderBy(a => a.SortOrder).Select(a => new QuizAnswer(a.Id, a.Text, a.AssetId, a.IsCorrect)).ToList())).ToList()
        };

        // A page with nothing filled in (a preview then shows only dashed placeholders).
        public static PageContent Empty(string templateType) =>
            new(Guid.Empty, templateType, null, null, new Dictionary<string, SlotContent>());

        // Example content for the template gallery and the template picker. Only texts, written as (key, text) pairs:
        //     PageContent.Sample("fact-hero", ("title", "Plastik i havet"), ("body", "..."))
        public static PageContent Sample(string templateType, params (string Key, string Text)[] texts) =>
            new(Guid.Empty, templateType, null, null, texts.ToDictionary(t => t.Key, t => new SlotContent(t.Text)));

        private static Dictionary<string, SlotContent> ToSlots(IEnumerable<(string Key, SlotContent Content)>? items)
        {
            var result = new Dictionary<string, SlotContent>();
            foreach (var (key, content) in items ?? [])
            {
                if (!string.IsNullOrEmpty(key)) result[key] = content;   // a duplicate key: the last one wins (never crashes)
            }
            return result;
        }
    }

    // Everything the components inside a template get from TemplateHost (as a cascading value).
    public sealed class TemplateScope(
        PageContent page,
        Template? definition,
        bool preview,
        bool placeholders,
        Guid? projectId,
        AssetUrls assets,
        EventCallback<Guid> navigate)
    {
        public PageContent Page { get; } = page;

        // The exhibition the page belongs to. Only the kiosk sets it: a quiz needs it to start a guest session.
        public Guid? ProjectId { get; } = projectId;

        // The template definition from the server, if known. Only used for placeholder labels ("Knap 1").
        public Template? Definition { get; } = definition;

        // Preview = nothing can be tapped (editor and thumbnails).
        public bool Preview { get; } = preview;

        // Placeholders = empty slots are drawn as dashed boxes, so staff can see where things go.
        public bool Placeholders { get; } = placeholders;

        // Called with the target page when a button is tapped (only the kiosk listens).
        public EventCallback<Guid> Navigate { get; } = navigate;

        public SlotContent? Slot(string key) => Page.Slots.GetValueOrDefault(key);

        public bool HasText(string key) => !string.IsNullOrWhiteSpace(Slot(key)?.Text);

        // True when at least one of the slots has a text or a file.
        // E.g. HasAny("stat1", "stat1Text", "stat1Image") = "is anything in card 1 filled in?"
        public bool HasAny(params string[] keys) => keys.Any(key => HasText(key) || Slot(key)?.AssetId is not null);

        // Should a group of slots (a card, a step, a column) be drawn? Yes if something in it is filled in,
        // and always in placeholder mode, so staff can see the empty boxes. Empty groups are left out,
        // which lets the remaining ones share the space.
        public bool Show(params string[] keys) => Placeholders || HasAny(keys);

        public string LabelOf(string key) =>
            Definition?.Slots?.FirstOrDefault(s => s.Key == key)?.Label ?? key;

        public string AssetUrl(Guid id) => assets.For(id);
    }
}
