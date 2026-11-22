using Domain.Entities;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IAssetInUseAsync
    {
        Task<bool> AssetInUseAsync(Guid assetId, CancellationToken ct);
    }
}