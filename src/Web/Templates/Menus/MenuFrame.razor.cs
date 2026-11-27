using Microsoft.AspNetCore.Components;

namespace Web.Templates.Menus
{
    public partial class MenuFrame
    {
        [CascadingParameter] public TemplateScope Scope { get; set; } = default!;

        // The button slots of this template, in order.
        [Parameter, EditorRequired] public IReadOnlyList<string> Keys { get; set; } = [];

        // Buttons per row.
        [Parameter] public int Columns { get; set; } = 2;

        [Parameter] public MenuButtonSize Size { get; set; } = MenuButtonSize.Medium;

        // Heading on the left, buttons on the right (instead of heading on top).
        [Parameter] public bool Split { get; set; }

        // Smaller heading, for menus with many buttons.
        [Parameter] public bool Compact { get; set; }

        private bool HasBackground => Scope.Slot("background")?.AssetId is not null;

        // With few buttons filled in, they share the width between them (never more columns than the template has).
        private int Cols => Scope.Placeholders
            ? Columns
            : Math.Clamp(Keys.Count(Scope.HasText), 1, Columns);

        private string SizeClass => "mf-" + Size.ToString().ToLowerInvariant();

        // button1, button2 ... buttonN. Must match TemplateCatalog on the server.
        public static string[] ButtonKeys(int count) =>
            Enumerable.Range(1, count).Select(i => $"button{i}").ToArray();

        private static readonly string[] SampleLabels =
            ["Quiz", "Fakta om affald", "Sorteringsspil", "Hav og strand", "Skoven", "Genbrug", "Dyrene", "Energi"];

        // Example content for the template gallery.
        public static PageContent Sample(string templateType, int buttons)
        {
            var slots = new Dictionary<string, SlotContent>
            {
                ["title"] = new("Velkommen til Økolariet"),
                ["intro"] = new("Tryk på en knap for at gå på opdagelse"),
            };

            for (var i = 0; i < buttons; i++)
                slots[$"button{i + 1}"] = new(SampleLabels[i], TargetId: Guid.NewGuid());

            return new(Guid.Empty, templateType, "Hovedmenu", null, slots);
        }
    }
}
