using Domain.Entities;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IGetAssetByIdAsync
    {
        Task<Asset?> GetAssetByIdAsync(Guid assetId, CancellationToken ct);
    }
}