using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAssetUseCases
{
    public interface IGetAssetUseCase
    {
        Task<Result<(Asset asset, Stream Content)>> GetAsync(Guid id, CancellationToken ct);
    }
}
