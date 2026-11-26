using Application.Helpers;
using Application.OutBoundPort.IAssetRepository;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Assets
{
    public sealed class ListAssetsAdapter(OkolarietDbContext db) : IListAssetsAsync
    {
        public async Task<PagedList<Asset>> ListAssetsAsync(AssetKind? kind, int page, int pageSize, CancellationToken ct)
        {
            var query = db.Assets.AsNoTracking();
            if (kind is not null)
                query = query.Where(a => a.Kind == kind);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.UploadedAt)   // newest first
                .ThenBy(a => a.Id)                       // fixed order, so pages don't overlap
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedList<Asset>(items, page, pageSize, total);
        }
    }
}