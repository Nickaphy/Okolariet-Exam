using Microsoft.AspNetCore.Components;

namespace Web.Templates.Slots
{
    // What every slot component (SlotText, SlotButton, SlotImage, SlotVideo, SlotAudio) has in common.
    // Each of them starts with  @inherits SlotBase  instead of declaring these three again.
    public abstract class SlotBase : ComponentBase
    {
        // The page being drawn, handed down by TemplateHost (a "cascading" value: every component inside gets it).
        [CascadingParameter] public TemplateScope Scope { get; set; } = default!;

        // Which slot this is, e.g. "title" or "button1". Must match a slot key in TemplateCatalog on the server.
        [Parameter, EditorRequired] public string Key { get; set; } = string.Empty;

        // CSS class from the template, which decides size and position.
        [Parameter] public string? Class { get; set; }

        // What staff filled into this slot, or null when it is empty.
        protected SlotContent? Content => Scope.Slot(Key);
    }
}
