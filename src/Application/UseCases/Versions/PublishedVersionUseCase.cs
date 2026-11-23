using System.Diagnostics;
using Application.ErrorMessages;
using Application.InBoundPort.IVersionUseCases;
using Application.OutBoundPort.IProjectRepository;
using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Versions
{
    // Publicerer et projekt: bygger et snapshot, giver det et versionsnummer og gemmer det.
    //
    // Afhængigheder (injiceres via konstruktøren):
    //  - getProject:     finder projektet
    //  - buildSnapshot:  bygger snapshottet og kontrollerer samtidig, at projektet kan publiceres
    //  - getLatest:      finder den seneste version, så næste versionsnummer kan beregnes
    //  - add:            gemmer den nye version
    //  - updateProject:  gemmer projektet med det nye seneste versionsnummer
    //  - time:           tidskilde. TimeProvider gør tid kontrollerbar i unit tests (registreret i ApplicationDependencyInjection)
    public class PublishVersionUseCase(
        IGetProjectByIdAsync getProject,
        IBuildSnapshotAsync buildSnapshot,
        IGetLatestVersionAsync getLatest,
        IAddVersionAsync add,
        IUpdateProjectAsync updateProject,
        TimeProvider time) : IPublishVersionUseCase
    {
        public async Task<Result<PublishedVersion>> PublishAsync(Guid projectId, CancellationToken ct)
        {
            try
            {
                // Projektet skal findes. Ellers HTTP 404.
                var project = await getProject.GetByIdAsync(projectId, ct);
                if (project is null) return Result.Fail<PublishedVersion>(new NotFoundError("Project not found."));

                // Byg snapshottet. Kan projektet ikke publiceres, returnerer BuildAsync en fejl (HTTP 422),
                // og vi stopper her uden at have gemt noget. ToResult<...>() videregiver fejlen i den rigtige Result-type.
                var snapshot = await buildSnapshot.BuildAsync(projectId, ct);
                if (snapshot.IsFailed) return snapshot.ToResult<PublishedVersion>();

                // Næste versionsnummer = seneste + 1. Er projektet aldrig publiceret, er latest null,
                // og "?? 0" giver 0 + 1 = 1.
                var latest = await getLatest.GetLatestAsync(projectId, ct);
                var next = (latest?.VersionNumber ?? 0) + 1;

                // Opret og gem versionen. Create validerer input.
                var version = PublishedVersion.Create(projectId, next, time.GetUtcNow(), snapshot.Value);
                await add.AddAsync(version, ct);

                // Opdater projektet, så det kender sit nyeste versionsnummer (vises som latestVersionNumber).
                // Trin 4 og 5 bør ske i samme databasetransaktion, så de ikke kan ende i uoverensstemmelse.
                // Det håndteres i Infrastructure.
                project.RegisterPublishedVersion(next);
                await updateProject.UpdateAsync(project, ct);

                return Result.Ok(version);
            }
            catch (Exception ex)
            {
                // Uventede fejl (fx databasen er utilgængelig) logges, men detaljerne vises ikke for brugeren.
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail<PublishedVersion>(new UnexpectedError("An unexpected error occurred while publishing."));
            }
        }
    }
}