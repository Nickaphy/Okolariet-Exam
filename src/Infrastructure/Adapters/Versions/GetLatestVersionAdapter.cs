using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Versions
{
    public sealed class GetLatestVersionAdapter(OkolarietDbContext db) : IGetLatestVersionAsync
    {
        public Task<PublishedVersion?> GetLatestAsync(Guid projectId, CancellationToken ct) =>
            db.PublishedVersions
                .AsNoTracking()
                .Where(v => v.ProjectId == projectId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(ct);
    }
}
