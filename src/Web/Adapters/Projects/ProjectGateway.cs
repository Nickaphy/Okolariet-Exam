using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Helpers;
using Web.Ports.Projects;

namespace Web.Adapters.Projects
{
    // Implementerer IProjectGateway ved at kalde API'et gennem IProjectsApi (genereret af Refit).
    // IProjectsApi leveres via dependency injection (registreret med AddApi<IProjectsApi>() i Program.cs).
    //
    // Alle Refit-kald returnerer et IApiResponse, som både kan indeholde data og en fejl:
    //  - Unwrap() returnerer dataen, hvis kaldet lykkedes, og kaster ellers en GatewayException med statuskoden.
    //  - EnsureSuccess() bruges, når svaret ikke har indhold (fx sletning): den kaster en GatewayException
    //    ved fejl og returnerer ellers ingenting.
    public class ProjectGateway(IProjectsApi api) : IProjectGateway
    {
        public async Task<PagedResult<Project>> ListProjects(int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var p = (await api.ListProjects(page, pageSize, ct)).Unwrap();

            // API'et svarer med en ProjectPage. Den pakkes om til vores egen PagedResult,
            // som siderne arbejder med. ToList() konverterer ICollection til en liste.
            return new PagedResult<Project>(p.Items.ToList(), p.Page, p.PageSize, p.TotalCount);
        }

        public async Task<Project> CreateProject(ProjectInput body, CancellationToken ct = default)
            => (await api.CreateProject(body, ct)).Unwrap();

        public async Task<Project> GetProject(Guid projectId, CancellationToken ct = default)
            => (await api.GetProject(projectId, ct)).Unwrap();

        // ETag'en står i svarets headers, ikke i selve projektet, så den hentes ud her.
        public async Task<Versioned<Project>> GetProjectWithETag(Guid projectId, CancellationToken ct = default)
        {
            var response = await api.GetProject(projectId, ct);
            return new Versioned<Project>(response.Unwrap(), response.Headers?.ETag?.Tag);
        }

        public async Task<Project> UpdateProject(Guid projectId, ProjectInput body, string ifMatch, CancellationToken ct = default)
            => (await api.UpdateProject(projectId, body, ifMatch, ct)).Unwrap();

        // Sletning returnerer intet indhold, derfor EnsureSuccess i stedet for Unwrap.
        public async Task DeleteProject(Guid projectId, CancellationToken ct = default)
            => (await api.DeleteProject(projectId, ct)).EnsureSuccess();
    }
}