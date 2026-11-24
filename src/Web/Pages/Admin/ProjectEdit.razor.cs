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
        private string? selectedPageType;
        private string? selectedMenuType;
        private int? publishedVersion;
        private Guid? loadedProjectId;

        private IReadOnlyList<Template> MenuTemplates =>
            model!.Templates.Where(t => t.Category == TemplateCategory.Menu).ToList();

        private IReadOnlyList<Template> PageTemplates =>
            model!.Templates.Where(t => t.Category != TemplateCategory.Menu).ToList(); // Note: != so it's all NOT main menu templates

        // The first menu is pre-selected, so the common case is a single click.
        private string? SelectedMenuType => selectedMenuType ?? MenuTemplates.FirstOrDefault()?.Type;

        private static string LabelId(ButtonPath button) => $"label-{button.Slot.Key}";

        private static string CategoryName(TemplateCategory category) => category switch //TO DO: Make sure to add Game when that type is ready
        {
            TemplateCategory.Quiz => "Quiz",
            TemplateCategory.Fact => "Fakta",
            _ => "Menu"
        };

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
            selectedPageType = null;
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

        // ---- results. ----
        // Each one closes the dialog, tells the user what happened and reloads.

        private Task OnRenamed(string name) => Finish("success", $"Udstillingen hedder nu «{name}».");

        private Task OnPageAdded(string title) => Finish("success", $"«{title}» er tilføjet.");

        private Task OnPageRemoved() => Finish("success", "Skabelonen er fjernet."); 

        private Task OnOrphanDeleted() => Finish("success", "Skabelonerne er slettet."); // All templates, that would be left behind if project get's deleted -> Orphans

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
            var error = await Editing.SaveButtonLabelAsync(model!, button, label);
            if (error is not null) notice = new Notice("error", error);

            // Reload either way: on success it shows the saved text, on failure it puts the old text back.
            await ReloadAsync();
            busy = false;
        }
    }
}
