using Application.OutBoundPort.IAssetRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Assets
{
    public sealed class AssetExistsAdapter(OkolarietDbContext db) : IAssetExistsAsync
    {
        public Task<bool> AssetExistsAsync(Guid assetId, CancellationToken ct) =>
            db.Assets.AnyAsync(a => a.Id == assetId, ct);
    }
}