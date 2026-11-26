using Application.OutBoundPort.IAssetRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Assets
{
    public sealed class AssetInUseAdapter(OkolarietDbContext db) : IAssetInUseAsync
    {
        // Checks all three tables that point to Assets. Soft-deleted questions/answers count too:
        // their foreign key is NoAction, so SQL Server would refuse the delete anyway.
        public async Task<bool> AssetInUseAsync(Guid assetId, CancellationToken ct) =>
            await db.ModdedTemplateItems.AnyAsync(i => i.AssetId == assetId, ct)
            || await db.Questions.AnyAsync(q => q.AssetId == assetId, ct)
            || await db.Answers.AnyAsync(a => a.AssetId == assetId, ct);
    }
}