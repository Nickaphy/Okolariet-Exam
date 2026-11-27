using Microsoft.JSInterop;
using Web.Api;
using Web.Api.Contracts;
using Web.Components;
using Web.Theming;

namespace Web.Pages.Admin
{
    public partial class Projects
    {
        private LoadState state = LoadState.Loading;
        private List<Project> projects = [];
        private string? loadError;
        private Notice? notice;
        private Project? projectToDelete;   // set = the delete dialog is open
        private bool showNewDialog;

        protected override Task OnInitializedAsync() => LoadAsync();

        private async Task LoadAsync()
        {
            state = LoadState.Loading;

            var (all, error) = await ApiCall.GetAll(page => ProjectGateway.ListProjects(page, 100));
            if (error is not null)
            {
                loadError = error;
                state = LoadState.Failed;
                return;
            }

            projects = all.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            state = LoadState.Loaded;
        }

        private async Task StartAsync(Project project)
        {
            var url = Nav.ToAbsoluteUri($"kiosk/{project.Id}").ToString();

            // One named window per project (wwwroot/js): pressing Start again brings the same window forward.
            var opened = await JS.InvokeAsync<bool>("oekolariet.openKioskWindow", url, $"kiosk-{project.Id:N}");

            notice = opened
                ? Notice.Info($"«{project.Name}» er åbnet i et nyt vindue. Flyt det over på touchskærmen, og tryk på skærmen for at starte i fuld skærm.")
                : Notice.Error("Browseren blokerede det nye vindue. Tillad pop op-vinduer for denne side, og prøv igen.");
        }

        private void Edit(Project project) => Nav.NavigateTo($"admin/projects/{project.Id}");

        // A new exhibition is empty, so go straight to the editor where it gets its content.
        private void OnProjectCreated(Project project)
        {
            showNewDialog = false;
            Nav.NavigateTo($"admin/projects/{project.Id}");
        }

        private void OnProjectDeleted(Project project)
        {
            projects.Remove(project);
            projectToDelete = null;
            notice = Notice.Success($"«{project.Name}» er slettet.");
        }

        // The little coloured square in front of each exhibition: its background colour, if it has one.
        // Theme.IsColour checks the value first, because it ends up in a style attribute.
        private static string SwatchStyle(Project project) =>
            Theme.IsColour(project.Appearance?.BackgroundColor) ? $"background:{project.Appearance!.BackgroundColor}" : string.Empty;
    }
}
