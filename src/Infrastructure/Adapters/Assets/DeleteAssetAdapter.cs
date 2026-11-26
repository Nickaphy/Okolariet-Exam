using Application.OutBoundPort.IAssetRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Assets
{
    public sealed class DeleteAssetAdapter(OkolarietDbContext db) : IDeleteAssetAsync
    {
        public async Task DeleteAssetAsync(Asset asset, CancellationToken ct)
        {
            db.Assets.Remove(asset);
            await db.SaveChangesAsync(ct);
        }
    }
}