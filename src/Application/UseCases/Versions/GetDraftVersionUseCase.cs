using Application.ErrorMessages;
using Application.InBoundPort.IVersionUseCases;
using Application.OutBoundPort.IProjectRepository;
using Application.OutBoundPort.IVersionRepository;
using Domain.ValueObjects;
using FluentResults;

namespace Application.UseCases.Versions
{
    // Forhåndsvisning: bygger et snapshot af projektets nuværende indhold uden at gemme det.
    // Lader personalet kontrollere resultatet, før der publiceres.
    public class GetDraftVersionUseCase(IGetProjectByIdAsync getProject, IBuildSnapshotAsync build) : IGetDraftVersionUseCase
    {
        public async Task<Result<VersionSnapshot>> GetDraftAsync(Guid projectId, CancellationToken ct)
        {
            // Projektet skal findes. Ellers HTTP 404.
            if (await getProject.GetByIdAsync(projectId, ct) is null)
                return Result.Fail<VersionSnapshot>(new NotFoundError("Project not found."));

            // BuildAsync returnerer enten snapshottet eller en fejl (HTTP 422), hvis projektet ikke kan vises endnu.
            // Resultatet gemmes ikke.
            return await build.BuildAsync(projectId, ct);
        }
    }
}