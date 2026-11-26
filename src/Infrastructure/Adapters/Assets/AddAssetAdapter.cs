using Application.OutBoundPort.IAssetRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Assets
{
    public sealed class AddAssetAdapter(OkolarietDbContext db) : IAddAssetAsync
    {
        public async Task AddAssetAsync(Asset asset, CancellationToken ct)
        {
            db.Assets.Add(asset);
            await db.SaveChangesAsync(ct);
        }
    }
}