using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Dialogs.EditorDialogs
{
    public partial class TemplatePickerDialog
    {
        [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
        [Parameter, EditorRequired] public IReadOnlyList<Template> Templates { get; set; } = [];

        // Small chips on a card ("Brugt på side 1"). They only inform.
        [Parameter] public Func<Template, IEnumerable<string>>? Notes { get; set; }

        [Parameter] public string SubmitText { get; set; } = "Tilføj side →";
        [Parameter] public string BusyText { get; set; } = "Tilføjer…";

        // Gets the chosen template type. Returns null on success, or a message to show.
        [Parameter, EditorRequired] public Func<string, Task<string?>> Submit { get; set; } = default!;

        // Called after a successful submit, with the chosen template type.
        [Parameter] public EventCallback<string> OnDone { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        private string? selected;
        private bool busy;
        private string? error;

        private bool CanSubmit => !busy && selected is not null;

        private void Select(string? type) => selected = type;

        private Task CancelAsync() => busy ? Task.CompletedTask : OnCancel.InvokeAsync();

        private async Task SubmitAsync()
        {
            if (!CanSubmit) return;

            var type = selected!;
            busy = true;
            error = null;
            error = await ApiCall.Try(() => Submit(type));   // null = it worked
            busy = false;

            if (error is null) await OnDone.InvokeAsync(type);
        }
    }
}