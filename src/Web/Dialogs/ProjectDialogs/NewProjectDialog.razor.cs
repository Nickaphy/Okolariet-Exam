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

        private async Task<string?> CreateAsync(string name)
        {
            var response = await ProjectsApi.CreateProject(new ProjectInput { Name = name });
            if (!response.IsSuccessStatusCode || response.Content is null)
                return ApiErrors.Describe(response);

            created = response.Content;
            return null;
        }

        private Task OnNamed(string _) => OnCreated.InvokeAsync(created!);
    }
}
