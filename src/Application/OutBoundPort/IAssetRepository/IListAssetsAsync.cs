using Domain.Entities;
using Application.Helpers;
using Domain.Enums;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IListAssetsAsync
    {
        Task<PagedList<Asset>> ListAssetsAsync(AssetKind? kind, int page, int pageSize, CancellationToken ct);
    }
}