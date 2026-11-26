using Application.OutBoundPort.IAssetRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Assets
{
    public sealed class GetAssetByIdAdapter(OkolarietDbContext db) : IGetAssetByIdAsync
    {
        // Tracked on purpose: Delete removes the returned asset in the same request.
        public Task<Asset?> GetAssetByIdAsync(Guid assetId, CancellationToken ct) =>
            db.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct);
    }
}