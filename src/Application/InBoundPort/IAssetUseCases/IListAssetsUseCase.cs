using Application.Helpers;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

namespace Application.InBoundPort.IAssetUseCases
{
    public interface IListAssetsUseCase
    {
        Task<Result<PagedList<Asset>>> ListAsync(AssetKind? kind, int page, int pageSize, CancellationToken ct);
    }
}