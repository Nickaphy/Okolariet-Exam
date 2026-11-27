using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Web.Components;

namespace Web.Dialogs.ReusableDialogs
{
    public partial class DialogShell
    {
        [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
        [Parameter] public DialogTone Tone { get; set; } = DialogTone.Normal;

        // While true (a save is running) Escape does nothing, so the dialog cannot vanish mid-save.
        [Parameter] public bool Busy { get; set; }

        // A wider dialog, for content like the template picker.
        [Parameter] public bool Wide { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? Actions { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private readonly string titleId = $"dlg-{Guid.NewGuid():N}";

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Escape" && !Busy) await OnClose.InvokeAsync();
        }
    }
}
