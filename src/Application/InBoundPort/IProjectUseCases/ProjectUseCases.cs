using Application.Commands.ProjectCommands;
using Application.Helpers;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IProjectUseCases
{
    // Inbound ports: det, controlleren kan bede Application om. Hver metode svarer til ét endpoint.
    // Selve logikken ligger i klasserne i UseCases/Projects.
    //
    // Result<> indeholder enten værdien eller en fejl, så forventede fejl (fx "findes ikke")
    // kan returneres uden exceptions. Result uden type bruges, hvor der ikke returneres en værdi.

    // Henter én side af projektlisten (paging).
    public interface IListProjectsUseCase { Task<Result<PagedList<Project>>> ListAsync(int page, int pageSize, CancellationToken ct); }

    // Henter ét projekt.
    public interface IGetProjectUseCase { Task<Result<Project>> GetAsync(Guid id, CancellationToken ct); }

    // Opretter et projekt og returnerer det.
    public interface ICreateProjectUseCase { Task<Result<Project>> CreateAsync(CreateProjectCommand command, CancellationToken ct); }

    // Opdaterer et projekt og returnerer det opdaterede projekt.
    public interface IUpdateProjectUseCase { Task<Result<Project>> UpdateAsync(UpdateProjectCommand command, CancellationToken ct); }

    // Sletter et projekt.
    public interface IDeleteProjectUseCase { Task<Result> DeleteAsync(DeleteProjectCommand command, CancellationToken ct); }
}