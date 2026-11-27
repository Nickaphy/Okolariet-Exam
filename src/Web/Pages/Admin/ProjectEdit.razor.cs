using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Services.Editor;

namespace Web.Pages.Admin
{
    public partial class ProjectEdit
    {
        // Different types of actions a user can make to a project. Extracted into Dialogs since many can be recycled
        private enum DialogKind { None, Rename, AddPage, RemovePage, DeleteOrphan, Publish }

        private sealed record Notice(string Kind, string Text);

        [Parameter] public Guid ProjectId { get; set; }

        // Model uses NEXT pattern since the Database can't handle that
        private ExhibitionModel? model;
        private string? loadError;
        private Notice? notice;
        private bool busy;

        private DialogKind dialog;
        private ButtonPath? activeButton;
        private int removeIndex;
        private ModdedTemplate? orphan;
        private string? selectedMenuType;
        private int? publishedVersion;
        private Guid? loadedProjectId;

        // Only a Menu template can be the main menu. Pages can use any template, menus included (they become sub-menus).
        private IReadOnlyList<Template> MenuTemplates =>
            model!.Templates.Where(t => t.Category == TemplateCategory.Menu).ToList();

        // The first menu is pre-selected, so the common case is a single click.
        private string? SelectedMenuType => selectedMenuType ?? MenuTemplates.FirstOrDefault()?.Type;

        private void SelectMenuType(string? type) => selectedMenuType = type;

        // "Faktaside · side 2 af 4": which template it is, and where it sits in its path.
        private string StepDetail(PathPage node, int index, int count)
        {
            var text = model!.DisplayNameOf(node.Page.TemplateType);
            if (node.IsMenu) text += " · undermenu";
            if (count > 1) text += $" · side {index + 1} af {count}";
            return text;
        }

        private static string LabelId(ButtonPath button) => $"label-{button.OwnerId:N}-{button.Slot.Key}";

        protected override async Task OnParametersSetAsync()
        {
            // Only start over when the route points at another project. Blazor can call this again
            // for the same project (e.g. when the layout re-renders), and that must not close dialogs.
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
                loadError = result.Error;
            }
            else
            {
                notice = new Notice("error", result.Error ?? ApiErrors.Offline);
            }
        }

        // ---- dialogs ----

        private void Open(DialogKind kind) => dialog = kind;

        private void CloseDialog()
        {
            dialog = DialogKind.None;
            activeButton = null;
            orphan = null;
        }

        private void AskAdd(ButtonPath button)
        {
            activeButton = button;
            dialog = DialogKind.AddPage;
        }

        private void AskRemove(ButtonPath button, int index)
        {
            activeButton = button;
            removeIndex = index;
            dialog = DialogKind.RemovePage;
        }

        private void AskDeleteOrphan(ModdedTemplate page)
        {
            orphan = page;
            dialog = DialogKind.DeleteOrphan;
        }

        // Small chips on the template cards. They only inform, they never block anything.
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

        // 1 → "1", 1,3 → "1 og 3", 1,3,4 → "1, 3 og 4"
        private static string JoinNumbers(IReadOnlyList<int> numbers) => numbers.Count == 1
            ? numbers[0].ToString()
            : string.Join(", ", numbers.Take(numbers.Count - 1)) + " og " + numbers[^1];

        // ---- results. ----
        // Each one closes the dialog, tells the user what happened and reloads.

        private Task OnRenamed(string name) => Finish("success", $"Udstillingen hedder nu «{name}».");

        private Task OnPageAdded(string templateType) =>
            Finish("success", $"«{model!.DisplayNameOf(templateType)}» er tilføjet.");

        private Task OnPageRemoved() => Finish("success", "Siden er fjernet.");

        private Task OnOrphanDeleted() => Finish("success", "Siderne er slettet."); // All pages that would be left behind if a project gets deleted -> Orphans

        private Task OnAppearanceSaved() => Finish("success", "Udseendet er gemt.");

        private Task OnPublished() => Finish("success", publishedVersion is { } v
            ? $"Udstillingen er udgivet som version {v}."
            : "Udstillingen er udgivet.");

        private async Task Finish(string kind, string text)
        {
            CloseDialog();
            notice = new Notice(kind, text);
            await ReloadAsync();
        }

        // ---- actions that are not dialogs ----

        private async Task<string?> PublishAsync()
        {
            var result = await Editing.PublishAsync(ProjectId);
            publishedVersion = result.Version;
            return result.Error;
        }

        private async Task CreateMenuAsync() //Main Menu is forced, no project will work without one
        {
            if (SelectedMenuType is not { } type) return; // Ensure one is selected

            busy = true;
            notice = null;
            var error = await Editing.CreateMenuAsync(model!, type);
            if (error is null) notice = new Notice("success", "Hovedmenuen er oprettet.");
            else notice = new Notice("error", error);
            await ReloadAsync();
            busy = false;
        }

        private async Task SaveLabelAsync(ButtonPath button, string label)
        {
            if (label.Trim() == button.Label) return;

            busy = true;
            notice = null;
            var error = await Editing.SaveButtonLabelAsync(button, label);
            if (error is not null) notice = new Notice("error", error);

            // Reload either way: on success it shows the saved text, on failure it puts the old text back.
            await ReloadAsync();
            busy = false;
        }
    }
}