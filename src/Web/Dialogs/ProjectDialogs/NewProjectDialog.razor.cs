using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Dialogs.ProjectDialogs
{
    public partial class NewProjectDialog
    {
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public EventCallback<Project> OnCreated { get; set; }

        private Project? created;

        // Handed to NameDialog as Submit: creates the project and remembers it for OnNamed below.
        private async Task<string?> CreateAsync(string name)
        {
            var (project, error) = await ApiCall.Get(() => ProjectGateway.CreateProject(new ProjectInput { Name = name }));
            created = project;
            return error;
        }

        // NameDialog only knows the name, so the created project is passed on from here.
        private Task OnNamed(string _) => OnCreated.InvokeAsync(created!);
    }
}
