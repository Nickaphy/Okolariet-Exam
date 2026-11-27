using Microsoft.AspNetCore.Components;

namespace Web.Templates.Menus
{
    // The shared frame of all main-menu templates (see MenuFrame.razor for the layout).
    // A menu template is just this frame with its own settings, e.g.
    //     <MenuFrame Buttons="6" Columns="3" Size="MenuButtonSize.Medium" Compact="true" />
    public partial class MenuFrame
    {
        [CascadingParameter] public TemplateScope Scope { get; set; } = default!;

        // How many buttons the template has. Their slot keys are button1, button2 ... (must match TemplateCatalog).
        [Parameter, EditorRequired] public int Buttons { get; set; }

        // Buttons per row.
        [Parameter] public int Columns { get; set; } = 2;

        [Parameter] public MenuButtonSize Size { get; set; } = MenuButtonSize.Medium;

        // Heading on the left, buttons on the right (instead of heading on top).
        [Parameter] public bool Split { get; set; }

        // Smaller heading, for menus with many buttons.
        [Parameter] public bool Compact { get; set; }

        private IEnumerable<string> Keys => ButtonKeys(Buttons);

        // Buttons per row actually used. With only 2 of 4 buttons filled in, those 2 share the whole width.
        // In placeholder mode every button is shown, so all columns are used.
        private int Cols => Scope.Placeholders
            ? Columns
            : Math.Clamp(Keys.Count(Scope.HasText), 1, Columns);

        // MenuButtonSize.Large → "mf-large" (the CSS class with that size).
        private string SizeClass => "mf-" + Size.ToString().ToLowerInvariant();

        private static IEnumerable<string> ButtonKeys(int count) =>
            Enumerable.Range(1, count).Select(i => $"button{i}");

        // ---- example content for the template gallery and the template picker ----

        private static readonly string[] SampleLabels =
            ["Quiz", "Fakta om affald", "Sorteringsspil", "Hav og strand", "Skoven", "Genbrug", "Dyrene", "Energi"];

        // Buttons get a made-up target, so they look "alive" (a button without a target is drawn faded).
        public static PageContent Sample(string templateType, int buttons)
        {
            var slots = new Dictionary<string, SlotContent>
            {
                ["title"] = new("Velkommen til Økolariet"),
                ["intro"] = new("Tryk på en knap for at gå på opdagelse"),
            };

            foreach (var (key, label) in ButtonKeys(buttons).Zip(SampleLabels))
                slots[key] = new SlotContent(label, TargetId: Guid.NewGuid());

            return new PageContent(Guid.Empty, templateType, null, null, slots);
        }
    }
}
