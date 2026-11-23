using Application.Commands.ProjectCommands;
using Application.ErrorMessages;
using Application.Helpers;
using Application.InBoundPort.IProjectUseCases;
using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using FluentResults;

// Henter én side af projektlisten (paging). Der er ingen forretningsregler at kontrollere,
// så use casen sender blot sidenummer og sidestørrelse videre til databasen.
// PagedList indeholder projekterne på siden samt sidenummer, sidestørrelse og samlet antal.
public class ListProjectsUseCase(IListProjectsAsync list) : IListProjectsUseCase
{
    public async Task<Result<PagedList<Project>>> ListAsync(int page, int pageSize, CancellationToken ct)
        => Result.Ok(await list.ListAsync(page, pageSize, ct));
}

public class GetProjectUseCase(IGetProjectByIdAsync getById) : IGetProjectUseCase
{
    public async Task<Result<Project>> GetAsync(Guid id, CancellationToken ct)
    {
        var project = await getById.GetByIdAsync(id, ct);
        return project is null
            ? Result.Fail<Project>(new NotFoundError("Project not found."))
            : Result.Ok(project);
    }
}

public class DeleteProjectUseCase(IGetProjectByIdAsync getById, IDeleteProjectAsync delete) : IDeleteProjectUseCase
{
    public async Task<Result> DeleteAsync(DeleteProjectCommand cmd, CancellationToken ct)
    {
        // Projektet kontrolleres først, så sletning af noget, der ikke findes, giver HTTP 404
        // i stedet for at lykkes uden at gøre noget.
        var project = await getById.GetByIdAsync(cmd.Id, ct);
        if (project is null) return Result.Fail(new NotFoundError("Project not found."));
        // Skærme, versioner og andet, der hører til projektet, slettes også.
        // Det sker via cascade delete i databasen og konfigureres i Infrastructure.
        await delete.DeleteAsync(project, ct);
        return Result.Ok();
    }
}