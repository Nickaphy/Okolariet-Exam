using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;
using Web.Templates;

namespace Web.Components.Editor
{
    public partial class TemplateCards
    {
        [Parameter, EditorRequired] public IReadOnlyList<Template> Templates { get; set; } = [];

        // The chosen template type. Use @bind-Selected.
        [Parameter] public string? Selected { get; set; }
        [Parameter] public EventCallback<string?> SelectedChanged { get; set; }

        // Chips for a card, e.g. "Brugt på side 1". Optional.
        [Parameter] public Func<Template, IEnumerable<string>>? Notes { get; set; }

        [Parameter] public string Legend { get; set; } = "Vælg skabelon";
        [Parameter] public bool ShowGroupHeadings { get; set; } = true;

        // The radio buttons of one group share a name. A new Guid makes it unique, also with two pickers on one page.
        private readonly string groupName = $"cards-{Guid.NewGuid():N}";

        private Task SelectAsync(string type) => SelectedChanged.InvokeAsync(type);

        private List<string> NotesOf(Template template) => Notes?.Invoke(template).ToList() ?? [];

        // The example content of the template. A template the app can't draw yet gets an empty page,
        // and TemplateHost then says it can't be shown.
        private static PageContent PreviewOf(Template template) =>
            TemplateRegistry.Find(template.Type)?.Sample() ?? PageContent.Empty(template.Type);
    }
}
