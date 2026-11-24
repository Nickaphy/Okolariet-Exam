using System.Net;
using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Dialogs.ProjectDialogs
{
    public partial class DeleteProjectDialog
    {
        [Parameter, EditorRequired] public Project Project { get; set; } = default!;
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public EventCallback<Project> OnDeleted { get; set; }

        private async Task<string?> DeleteAsync()
        {
            var response = await ProjectsApi.DeleteProject(Project.Id);

            // 404: someone else already deleted it. The end result is what the user wanted.
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound
                ? null
                : ApiErrors.Describe(response);
        }
    }
}
