using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Components;
using Web.Services.Editor;
using Web.Templates;

namespace Web.Pages.Admin
{
    public partial class ProjectEdit
    {
        // Which dialog is open. Only one at a time; None = no dialog.
        private enum DialogKind { None, Rename, AddPage, RemovePage, DeleteOrphan, Publish }

        [Parameter] public Guid ProjectId { get; set; }

        // Everything shown on the page (see ExhibitionModel). null until loaded.
        private ExhibitionModel? model;
        private string? loadError;
        private Notice? notice;

        // True while something is being saved. Locks the fields, so the user can't start two saves at once.
        private bool busy;

        // The open dialog, and what it is about.
        private DialogKind dialog;
        private ButtonPath? activeButton;   // AddPage and RemovePage: which button's path
        private int removeIndex;            // RemovePage: which page in that path (0-based)
        private ModdedTemplate? orphan;     // DeleteOrphan: which page
        private int? publishedVersion;      // Publish: the new version number, for the success message

        private string? selectedMenuType;
        private Guid? loadedProjectId;

        // ---- things worked out from the model ----

        // The main menu as the template components want it (for the preview and the Udseende preview).
        private PageContent? MenuPage => model?.Menu is null ? null : PageContent.From(model.Menu);

        // Only a Menu template can be the main menu. Pages can use any template, menus included (they become sub-menus).
        private IReadOnlyList<Template> MenuTemplates =>
            model!.Templates.Where(t => t.Category == TemplateCategory.Menu).ToList();

        // The first menu template is pre-selected, so the common case is a single click.
        private string? SelectedMenuType => selectedMenuType ?? MenuTemplates.FirstOrDefault()?.Type;

        private void SelectMenuType(string? type) => selectedMenuType = type;

        // One text field per text slot of the main menu's template (heading, intro ...).
        private IReadOnlyList<SlotDefinition> MenuTextSlots =>
            model?.MenuTemplate?.Slots?.Where(s => s.Kind == SlotDefinitionKind.Text).ToList() ?? [];

        private string MenuText(string slotKey) =>
            model?.Menu?.Items?.FirstOrDefault(i => i.SlotKey == slotKey)?.Text ?? string.Empty;

        // ---- loading ----

        protected override async Task OnParametersSetAsync()
        {
            // Only start over when the address points at another project. Blazor can call this again
            // for the same project (e.g. when the layout re-renders), and that must not close an open dialog.
            if (loadedProjectId == ProjectId) return;
            loadedProjectId = ProjectId;

            model = null;
            loadError = null;
            notice = null;
            dialog = DialogKind.None;
            await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            var result = await Loader.LoadAsync(ProjectId);

            if (result.Model is not null)
            {
                model = result.Model;
                loadError = null;
            }
            else if (model is null)
            {
                loadError = result.Error;   // nothing to show yet → the whole page shows the error
            }
            else
            {
                notice = Notice.Error(result.Error ?? ApiErrors.Offline);   // keep showing the old model, with a message
            }
        }

        // ---- saving straight from the page (no dialog) ----

        // The pattern every direct save follows: lock the page, save, show the error (or the success text), reload.
        // Reloading also on failure puts the saved value back into a field the user had changed.
        private async Task SaveAsync(Func<Task<string?>> save, string? successText = null)
        {
            busy = true;
            notice = null;

            var error = await save();
            if (error is not null) notice = Notice.Error(error);
            else if (successText is not null) notice = Notice.Success(successText);

            await ReloadAsync();
            busy = false;
        }

        // The main menu is required: nothing else can be added before it exists.
        private Task CreateMenuAsync() => SelectedMenuType is { } type
            ? SaveAsync(() => Editing.CreateMenuAsync(model!, type), "Hovedmenuen er oprettet.")
            : Task.CompletedTask;

        private Task SaveMenuTextAsync(SlotDefinition slot, string text) =>
            model?.Menu is null || text.Trim() == MenuText(slot.Key)
                ? Task.CompletedTask   // nothing changed
                : SaveAsync(() => Editing.SaveTextAsync(model!.Menu!.Id, slot.Key, text));

        // From ButtonPathList: the text on a button was changed.
        private Task SaveLabelAsync((ButtonPath Button, string Label) change) =>
            change.Label.Trim() == change.Button.Label
                ? Task.CompletedTask   // nothing changed
                : SaveAsync(() => Editing.SaveButtonLabelAsync(change.Button, change.Label));

        // ---- dialogs: opening and closing ----

        private void Open(DialogKind kind) => dialog = kind;

        private void CloseDialog()
        {
            dialog = DialogKind.None;
            activeButton = null;
            orphan = null;
        }

        // From ButtonPathList: "Tilføj side" was pressed.
        private void AskAdd(ButtonPath button)
        {
            activeButton = button;
            dialog = DialogKind.AddPage;
        }

        // From ButtonPathList: "Fjern" was pressed on a page.
        private void AskRemove((ButtonPath Button, int Index) page)
        {
            activeButton = page.Button;
            removeIndex = page.Index;
            dialog = DialogKind.RemovePage;
        }

        private void AskDeleteOrphan(ModdedTemplate page)
        {
            orphan = page;
            dialog = DialogKind.DeleteOrphan;
        }

        // Handed to ConfirmDialog as Run: publishing also gives back the version number.
        private async Task<string?> PublishAsync()
        {
            var result = await Editing.PublishAsync(ProjectId);
            publishedVersion = result.Version;
            return result.Error;
        }

        // Small chips on the template cards in the "add page" dialog. They only inform, they never block anything.
        private IEnumerable<string> NotesFor(Template template)
        {
            if (activeButton is null) yield break;

            var lastIsThis = activeButton.LastIs(template.Type);
            var earlier = activeButton.PositionsOf(template.Type)
                .Where(number => !(lastIsThis && number == activeButton.Pages.Count))
                .ToList();

            if (earlier.Count > 0) yield return "Brugt på side " + JoinNumbers(earlier);
            if (lastIsThis) yield return "Brugt på forrige side";
            if (template.Category == TemplateCategory.Menu) yield return "Undermenu";
        }

        // 1 → "1",  1,3 → "1 og 3",  1,3,4 → "1, 3 og 4"
        private static string JoinNumbers(IReadOnlyList<int> numbers) => numbers.Count == 1
            ? numbers[0].ToString()
            : string.Join(", ", numbers.Take(numbers.Count - 1)) + " og " + numbers[^1];

        // ---- dialogs: after a successful save ----
        // Each one closes the dialog, says what happened and reloads.

        private Task OnRenamed(string name) => Finish($"Udstillingen hedder nu «{name}».");

        private Task OnPageAdded(string templateType) => Finish($"«{model!.DisplayNameOf(templateType)}» er tilføjet.");

        private Task OnPageRemoved() => Finish("Siden er fjernet.");

        private Task OnOrphanDeleted() => Finish("Siden er slettet.");

        private Task OnAppearanceSaved() => Finish("Udseendet er gemt.");

        private Task OnPublished() => Finish(publishedVersion is { } v
            ? $"Udstillingen er udgivet som version {v}."
            : "Udstillingen er udgivet.");

        private async Task Finish(string successText)
        {
            CloseDialog();
            notice = Notice.Success(successText);
            await ReloadAsync();
        }
    }
}
