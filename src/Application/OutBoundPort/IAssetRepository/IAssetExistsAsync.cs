using Domain.Entities;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IAssetExistsAsync
    {
        Task<bool> AssetExistsAsync(Guid assetId, CancellationToken ct);
    }
}