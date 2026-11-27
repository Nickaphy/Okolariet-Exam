using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;

namespace Web.Components.Sketches
{
    public partial class TemplateCards
    {
        [Parameter, EditorRequired] public IReadOnlyList<Template> Templates { get; set; } = [];
        [Parameter] public string? Selected { get; set; }
        [Parameter] public EventCallback<string?> SelectedChanged { get; set; }
        [Parameter] public Func<Template, IEnumerable<string>>? Notes { get; set; }
        [Parameter] public string Legend { get; set; } = "Vælg skabelon";
        [Parameter] public bool ShowGroupHeadings { get; set; } = true;

        private readonly string groupName = $"cards-{Guid.NewGuid():N}"; //Name it with random Guid to ensure unique

        private async Task SelectAsync(string type)
        {
            await SelectedChanged.InvokeAsync(type);
        }
    }
}
