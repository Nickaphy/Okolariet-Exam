using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Templates;
using Web.Theming;

namespace Web.Components
{
    public partial class AppearanceEditor
    {
        // What is saved now. null = nothing chosen.
        [Parameter] public Appearance? Value { get; set; }

        // What applies where this Appearance is empty (the project's look, when editing one page). null = the neutral default.
        [Parameter] public Appearance? Inherited { get; set; }

        // The page shown in the preview (e.g. the main menu). Without one, the first template's example content is used.
        [Parameter] public PageContent? PreviewPage { get; set; }
        [Parameter] public Template? PreviewDefinition { get; set; }

        // Saves. Gets null when nothing is chosen. Returns null on success, or an error message (like the dialogs).
        [Parameter, EditorRequired] public Func<Appearance?, Task<string?>> Save { get; set; } = default!;
        [Parameter] public EventCallback OnSaved { get; set; }

        // What the user is choosing right now (not saved yet), and what was saved.
        // Look is a record, so == compares the four values. That makes "has anything changed?" and
        // "is this preset chosen?" one comparison each.
        private Look draft = Look.Empty;
        private Look saved = Look.Empty;

        private bool busy;
        private string? error;

        private bool Dirty => draft != saved;

        private bool IsChosen(AppearancePreset preset) => draft == Look.From(preset.Appearance);

        // The colours to show: the chosen ones, and what would apply where nothing is chosen.
        private Appearance Resolved => Theme.Resolve(draft.ToAppearance(), Inherited);

        // The preview page drawn with the colours being edited.
        private PageContent PreviewWithLook =>
            (PreviewPage ?? TemplateRegistry.All[0].Sample()) with { Appearance = Resolved };

        protected override void OnParametersSet()
        {
            // Start over only when a different saved value arrives (after a save and reload).
            // Blazor also calls this when the parent re-renders for other reasons; that must not throw away the draft.
            var incoming = Look.From(Value);
            if (incoming == saved) return;

            saved = incoming;
            draft = incoming;
            error = null;
        }

        private void Apply(AppearancePreset preset) => draft = Look.From(preset.Appearance);

        private void Revert()
        {
            draft = saved;
            error = null;
        }

        private async Task SaveAsync()
        {
            busy = true;
            error = null;
            error = await ApiCall.Try(() => Save(draft.ToAppearance()));
            busy = false;

            if (error is null) await OnSaved.InvokeAsync();
        }

        // The four choices, cleaned up so they compare well: empty text → null, colours in upper case.
        private sealed record Look(string? Background, string? Button, string? Text, string? Font)
        {
            public static readonly Look Empty = new(null, null, null, null);

            public static Look From(Appearance? a) => new(
                Colour(a?.BackgroundColor), Colour(a?.ButtonColor), Colour(a?.TextColor), Clean(a?.FontFamily));

            // null when nothing is chosen, so the saved Appearance is null too (= "use the default").
            public Appearance? ToAppearance() => this == Empty
                ? null
                : new Appearance { BackgroundColor = Background, ButtonColor = Button, TextColor = Text, FontFamily = Font };

            private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            private static string? Colour(string? value) => Clean(value)?.ToUpperInvariant();
        }
    }
}
