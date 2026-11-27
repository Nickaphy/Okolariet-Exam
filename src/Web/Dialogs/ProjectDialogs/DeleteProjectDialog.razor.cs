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

        // Handed to ConfirmDialog as Run. ignoreNotFound: if someone else already deleted it, that is fine too.
        private Task<string?> DeleteAsync() =>
            ApiCall.Run(() => ProjectGateway.DeleteProject(Project.Id), ignoreNotFound: true);
    }
}
