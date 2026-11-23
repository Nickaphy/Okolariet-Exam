using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Pages.Admin
{
    public partial class Projects
    {
        private enum LoadState { Loading, Loaded, Failed }

        private sealed record Notice(string Kind, string Text);

        private static readonly Regex HexColour = new("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$");

        private LoadState state = LoadState.Loading;
        private List<Project> projects = [];
        private string? loadError;
        private Notice? notice;
        private Project? projectToDelete;

        protected override Task OnInitializedAsync() => LoadAsync();

        private async Task LoadAsync()
        {
            state = LoadState.Loading;
            try
            {
                // The list is paged (max 100 per page); a museum has few projects, so fetch them all.
                var all = new List<Project>();
                for (var page = 1; ; page++)
                {
                    var response = await ProjectsApi.ListProjects(page, 100);
                    if (!response.IsSuccessStatusCode || response.Content is null)
                    {
                        loadError = ApiErrors.Describe(response);
                        state = LoadState.Failed;
                        return;
                    }

                    all.AddRange(response.Content.Items);
                    if (response.Content.Items.Count == 0 || all.Count >= response.Content.TotalCount) break;
                }

                projects = all.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
                state = LoadState.Loaded;
            }
            catch (HttpRequestException)
            {
                loadError = ApiErrors.Offline;
                state = LoadState.Failed;
            }
        }

        private async Task StartAsync(Project project)
        {
            var url = Nav.ToAbsoluteUri($"kiosk/{project.Id}").ToString();

            // One named window per project: pressing Start again brings the same window forward.
            var opened = await JS.InvokeAsync<bool>("oekolariet.openKioskWindow", url, $"kiosk-{project.Id:N}");

            notice = opened
                ? new Notice("info", $"«{project.Name}» er åbnet i et nyt vindue. Flyt det over på touchskærmen, og tryk på skærmen for at starte i fuld skærm.")
                : new Notice("error", "Browseren blokerede det nye vindue. Tillad pop op-vinduer for denne side, og prøv igen.");
        }

        private void Edit(Project project) => Nav.NavigateTo($"admin/projects/{project.Id}");

        private void OnProjectDeleted(Project project)
        {
            projects.Remove(project);
            projectToDelete = null;
            notice = new Notice("success", $"«{project.Name}» er slettet.");
        }

        private static string SwatchStyle(Project project) =>
            project.Appearance?.BackgroundColor is { } colour && HexColour.IsMatch(colour)
                ? $"background:{colour}"
                : string.Empty;
    }
}
