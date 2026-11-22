using Application.OutBoundPort.IAssetRepository;
using Application.InBoundPort.IAssetUseCases;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Application.Helpers;

namespace Application.UseCases.Assets
{
    public class ListAssetsUseCase(IListAssetsAsync listAssets) : IListAssetsUseCase
    {
        public async Task<Result<PagedList<Asset>>> ListAsync(AssetKind? kind, int page, int pageSize, CancellationToken ct)
            => Result.Ok(await listAssets.ListAssetsAsync(kind, page, pageSize, ct));
    }
}