using Refit;
using Web.Api;
using Web.Api.Contracts;
using Web.Exceptions;
using Web.Helpers;
using Web.Ports.Assets;
using Web.Ports.Helpers;

//In here, if the API sends something back, it will unwrap; if it sends nothing, it will delete it and ensure it did (EnsureSuccess).
namespace Web.Adapters.Assets
{
    public class AssetGateway(IAssetsApi api) : IAssetGateway
    {
        public async Task<PagedResult<Asset>> ListAssets(AssetKind? kind, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var p = (await api.ListAssets(kind, page, pageSize, ct)).Unwrap();
            return new PagedResult<Asset>(p.Items.ToList(), p.Page, p.PageSize, p.TotalCount);
        }

        public async Task<Asset> UploadAsset(Stream content, string fileName, string contentType, CancellationToken ct = default)
        => (await api.UploadAsset(new StreamPart(content, fileName, contentType), ct)).Unwrap();

        public async Task<Stream> GetAsset(Guid assetId, string? range = null, CancellationToken ct = default)
        {
            var res = await api.GetAsset(assetId, range!, ct);
            if (!res.IsSuccessStatusCode) throw new GatewayException(res.StatusCode);
            return await res.Content.ReadAsStreamAsync(ct);
        }
        public async Task DeleteAsset(Guid assetId, CancellationToken ct = default)
        => (await api.DeleteAsset(assetId, ct)).EnsureSuccess();
    }
}