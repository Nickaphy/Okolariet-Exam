using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Theming;

namespace Web.Components
{
    public partial class AppearanceEditor
    {
        // What is saved now. Null means nothing is chosen.
        [Parameter] public Appearance? Value { get; set; }

        // What applies when a value here is left empty (the project's look, when editing a page). Null = the neutral default.
        [Parameter] public Appearance? Inherited { get; set; }

        [Parameter, EditorRequired] public Func<Appearance?, Task<string?>> Save { get; set; } = default!;
        [Parameter] public EventCallback OnSaved { get; set; }

        private readonly string idBg = $"ap-{Guid.NewGuid():N}";
        private readonly string idButton = $"ap-{Guid.NewGuid():N}";
        private readonly string idText = $"ap-{Guid.NewGuid():N}";
        private readonly string idFont = $"ap-{Guid.NewGuid():N}";

        private string? bg, button, text, font;
        private string? loadedKey;
        private bool busy;
        private string? error;

        // What the pickers show: the chosen value, else what would apply.
        private Appearance resolved => Theme.Resolve(Current(), Inherited);

        protected override void OnParametersSet()
        {
            // Start over from the saved value when a different one arrives (after a save and reload).
            // Compared by content, so an unrelated reload doesn't throw away what is being edited.
            var key = string.Join('|', Value?.BackgroundColor, Value?.ButtonColor, Value?.TextColor, Value?.FontFamily);
            if (key == loadedKey) return;

            loadedKey = key;
            bg = Empty(Value?.BackgroundColor);
            button = Empty(Value?.ButtonColor);
            text = Empty(Value?.TextColor);
            font = Empty(Value?.FontFamily);
        }

        private Appearance? Current() =>
            bg is null && button is null && text is null && font is null
                ? null
                : new Appearance { BackgroundColor = bg, ButtonColor = button, TextColor = text, FontFamily = font };

        private bool Dirty =>
            !Same(bg, Value?.BackgroundColor) || !Same(button, Value?.ButtonColor) ||
            !Same(text, Value?.TextColor) || !Same(font, Value?.FontFamily);

        private bool IsPreset(AppearancePreset p) =>
            Same(bg, p.Appearance.BackgroundColor) && Same(button, p.Appearance.ButtonColor) &&
            Same(text, p.Appearance.TextColor) && Same(font, p.Appearance.FontFamily);

        private void Apply(AppearancePreset p)
        {
            bg = Empty(p.Appearance.BackgroundColor);
            button = Empty(p.Appearance.ButtonColor);
            text = Empty(p.Appearance.TextColor);
            font = Empty(p.Appearance.FontFamily);
        }

        private static string? Colour(ChangeEventArgs e) => Theme.IsColour(e.Value?.ToString()) ? e.Value!.ToString() : null;

        private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static bool Same(string? a, string? b) =>
            string.Equals(Empty(a), Empty(b), StringComparison.OrdinalIgnoreCase);

        private async Task SaveAsync()
        {
            busy = true;
            error = null;
            try
            {
                error = await Save(Current());
            }
            catch (HttpRequestException)
            {
                error = ApiErrors.Offline;
            }
            busy = false;

            if (error is null) await OnSaved.InvokeAsync();
        }
    }
}
