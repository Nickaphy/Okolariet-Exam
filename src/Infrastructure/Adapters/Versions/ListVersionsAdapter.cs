using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Versions
{
    public sealed class ListVersionsAdapter(OkolarietDbContext db) : IListVersionsAsync
    {
        public async Task<IReadOnlyList<PublishedVersion>> ListAsync(Guid projectId, CancellationToken ct) =>
            await db.PublishedVersions
                .AsNoTracking()
                .Where(v => v.ProjectId == projectId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync(ct);
    }
}
