using Microsoft.AspNetCore.Components;
using Web.Api;

namespace Web.Dialogs.ReusableDialogs
{
    public partial class NameDialog
    {
        [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
        [Parameter] public string Label { get; set; } = "Navn";
        [Parameter] public string? Placeholder { get; set; }
        [Parameter] public string? Hint { get; set; }
        // "text" for names. Use "password" or "email" when the field holds one of those.
        [Parameter] public string InputType { get; set; } = "text";


        // Text the field starts with (e.g. the current name when renaming).
        [Parameter] public string? Value { get; set; }
        [Parameter] public int MaxLength { get; set; } = 120;

        [Parameter] public string SubmitText { get; set; } = "Gem";
        [Parameter] public string BusyText { get; set; } = "Gemmer…";


        // Extra requirements outside the name field. False keeps the button locked.
        [Parameter] public bool Valid { get; set; } = true;

        [Parameter] public RenderFragment? Extra { get; set; }

        // Does the work with the typed text. Returns null on success, or a message to show in the dialog.
        [Parameter, EditorRequired] public Func<string, Task<string?>> Submit { get; set; } = default!;

        // Called after a successful submit, with the name that was saved.
        [Parameter] public EventCallback<string> OnDone { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        private readonly string formId = $"name-form-{Guid.NewGuid():N}";
        private readonly string inputId = $"name-{Guid.NewGuid():N}";
        private ElementReference nameInput;
        private string text = string.Empty;
        private bool busy;
        private string? error;

        private bool CanSubmit => !busy && Valid && text.Trim().Length > 0;

        // Start with the current name when renaming (never null, so .Trim() is always safe).
        protected override void OnInitialized() => text = Value ?? string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) await nameInput.FocusAsync();
        }

        private Task CancelAsync() => busy ? Task.CompletedTask : OnCancel.InvokeAsync();

        private async Task SubmitAsync()
        {
            if (!CanSubmit) return;

            var name = text.Trim();
            busy = true;
            error = null;
            error = await ApiCall.Try(() => Submit(name));   // null = it worked
            busy = false;

            if (error is null) await OnDone.InvokeAsync(name);
        }
    }
}
