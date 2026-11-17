using Domain.Entities;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IAddAssetAsync
    {
        Task AddAssetAsync(Asset asset, CancellationToken ct);
    }
}