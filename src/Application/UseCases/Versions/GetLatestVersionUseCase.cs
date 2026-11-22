using Application.ErrorMessages;
using Application.InBoundPort.IVersionUseCases;
using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Versions
{
    // Henter den seneste publicerede version af et projekt. Kaldes af kiosken.
    public class GetLatestVersionUseCase(IGetLatestVersionAsync getLatest) : IGetLatestVersionUseCase
    {
        public async Task<Result<PublishedVersion>> GetLatestAsync(Guid projectId, CancellationToken ct)
        {
            var version = await getLatest.GetLatestAsync(projectId, ct);

            return version is null
                ? Result.Fail<PublishedVersion>(new NotFoundError("No published version found."))
                : Result.Ok(version);
        }
    }
}