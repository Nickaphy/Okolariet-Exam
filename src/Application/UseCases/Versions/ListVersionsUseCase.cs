using Application.ErrorMessages;
using Application.InBoundPort.IVersionUseCases;
using Application.OutBoundPort.IProjectRepository;
using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Versions
{
    // Returnerer historikken over alle versioner af et projekt, nyeste først.
    public class ListVersionsUseCase(IGetProjectByIdAsync getProject, IListVersionsAsync list) : IListVersionsUseCase
    {
        public async Task<Result<IReadOnlyList<PublishedVersion>>> ListAsync(Guid projectId, CancellationToken ct)
        {
            // Projektet kontrolleres først. Uden kontrollen ville et ukendt projekt give en tom liste
            // i stedet for HTTP 404, og klienten kunne ikke se forskel på "ingen versioner" og "findes ikke".
            if (await getProject.GetByIdAsync(projectId, ct) is null)
                return Result.Fail<IReadOnlyList<PublishedVersion>>(new NotFoundError("Project not found."));

            return Result.Ok(await list.ListAsync(projectId, ct));
        }
    }
}