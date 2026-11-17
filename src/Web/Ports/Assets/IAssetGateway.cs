using Web.Api.Contracts;
using Web.Ports.Helpers;

namespace Web.Ports.Assets
{
    public interface IAssetGateway
    {
        Task<PagedResult<Asset>> ListAssets(AssetKind? kind, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<Asset> UploadAsset(Stream content, string fileName, string contentType, CancellationToken ct = default);
        Task<Stream> GetAsset(Guid assetId, string? range = null, CancellationToken ct = default);
        Task DeleteAsset(Guid assetId, CancellationToken ct = default);
    }
}