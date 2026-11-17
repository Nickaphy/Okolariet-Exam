using Domain.Entities;

namespace Application.OutBoundPort.IAssetRepository
{
    public interface IDeleteAssetAsync
    {
        Task DeleteAssetAsync(Asset asset, CancellationToken ct);
    }
}