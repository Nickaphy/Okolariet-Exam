using Web.Api.Contracts;
using Web.Ports.Helpers;

namespace Web.Ports.Projects
{
    // Kontrakten mellem Blazor-siderne og API'et for projekter. Siderne kender kun dette interface
    // og ved ikke noget om HTTP eller Refit.
    // Metoderne svarer til IProjectsApi i Generated/Output.cs, men uden IApiResponse
    // (fejl håndteres i adapteren), og ICollection er erstattet af IReadOnlyList/PagedResult.
    public interface IProjectGateway
    {
        // Henter én side af projektlisten (paging). Listen deles op i bidder på pageSize projekter,
        // og page angiver hvilken bid der hentes, fx page 2 med pageSize 20 giver projekt 21-40.
        // Standardværdierne er side 1 med 20 projekter.
        // PagedResult indeholder både projekterne og oplysninger om antal og sider (HasNext, HasPrevious),
        // som bruges til Forrige/Næste-knapper.
        // CancellationToken gør det muligt at afbryde kaldet, fx hvis brugeren forlader siden.
        Task<PagedResult<Project>> ListProjects(int page = 1, int pageSize = 20, CancellationToken ct = default);

        // Opretter et projekt ud fra de oplysninger, brugeren har indtastet (body).
        // Returnerer det oprettede projekt, inklusive det id API'et har tildelt.
        Task<Project> CreateProject(ProjectInput body, CancellationToken ct = default);

        // Henter ét projekt ud fra id.
        Task<Project> GetProject(Guid projectId, CancellationToken ct = default);

        // Opdaterer et projekt. ifMatch er projektets ETag (en versionsmærkat) fra sidste hentning.
        // API'et kræver den for at undgå, at to brugere overskriver hinandens ændringer:
        // hvis mærkaten ikke matcher, er projektet ændret af en anden i mellemtiden.
        Task<Project> UpdateProject(Guid projectId, ProjectInput body, string ifMatch, CancellationToken ct = default);

        // Sletter et projekt. Returtypen er Task uden værdi, fordi API'et ikke returnerer noget indhold ved sletning.
        Task DeleteProject(Guid projectId, CancellationToken ct = default);
    }
}