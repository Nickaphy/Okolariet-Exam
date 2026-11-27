using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Web.Api;
using Web.Components;

namespace Web.Dialogs.ReusableDialogs
{
    public partial class ConfirmDialog
    {
        [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
        [Parameter] public DialogTone Tone { get; set; } = DialogTone.Normal;
        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter] public string ConfirmText { get; set; } = "Bekræft";
        [Parameter] public string BusyText { get; set; } = "Vent…";
        [Parameter] public string CancelText { get; set; } = "Annuller";

        // When set, the confirm button stays locked until this text is typed (case-insensitive).
        [Parameter] public string? RequireTyped { get; set; }

        // Does the work. Return null for success, or a message to show the user.
        [Parameter, EditorRequired] public Func<Task<string?>> Run { get; set; } = default!;

        [Parameter] public EventCallback OnDone { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        private readonly string inputId = $"confirm-{Guid.NewGuid():N}";
        private ElementReference typedInput;
        private ElementReference cancelButton;
        private string typed = string.Empty;
        private bool busy;
        private string? error;

        private bool CanConfirm =>
            !busy && (RequireTyped is null ||
                      string.Equals(typed.Trim(), RequireTyped.Trim(), StringComparison.CurrentCultureIgnoreCase));

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            // With a typed confirmation the cursor goes in the field. Otherwise it goes on
            // "Annuller", so a stray Enter press can never confirm something destructive.
            if (RequireTyped is not null) await typedInput.FocusAsync();
            else await cancelButton.FocusAsync();
        }

        private Task CancelAsync() => busy ? Task.CompletedTask : OnCancel.InvokeAsync();

        private async Task OnTypedKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && CanConfirm) await ConfirmAsync();
        }

        private async Task ConfirmAsync()
        {
            if (!CanConfirm) return;

            busy = true;
            error = null;
            error = await ApiCall.Try(Run);   // null = it worked
            busy = false;

            if (error is null) await OnDone.InvokeAsync();
        }
    }
}
