using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;
using Web.Services.Editor;

namespace Web.Components.Sketches
{
    public partial class TemplateSketch
    {
        [Parameter, EditorRequired] public Template Template { get; set; } = default!;

        private bool headline;
        private int bodyLines;
        private int buttons;
        private int audio;
        private List<SlotDefinition> media = [];

        protected override void OnParametersSet()
        {
            var slots = Template.Slots.Where(s => s.Key != PathConvention.NextSlotKey).ToList();
            var texts = slots.Count(s => s.Kind == SlotDefinitionKind.Text);

            headline = texts > 0;                 // the first text slot is drawn as the headline
            bodyLines = Math.Max(texts - 1, 0);   // the rest as lines of text
            buttons = slots.Count(s => s.Kind == SlotDefinitionKind.Button);
            audio = slots.Count(s => s.Kind == SlotDefinitionKind.Audio);
            media = slots.Where(s => s.Kind is SlotDefinitionKind.Image or SlotDefinitionKind.Video).ToList();
        }
    }
}
