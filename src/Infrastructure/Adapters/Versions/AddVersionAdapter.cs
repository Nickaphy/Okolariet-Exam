using Application.OutBoundPort.IVersionRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Versions
{
    public sealed class AddVersionAdapter(OkolarietDbContext db) : IAddVersionAsync
    {
        // If two people publish the same project at once, the unique index on (ProjectId, VersionNumber)
        // makes the second save fail instead of creating a duplicate version number.
        public async Task AddAsync(PublishedVersion version, CancellationToken ct)
        {
            db.PublishedVersions.Add(version);
            await db.SaveChangesAsync(ct);
        }
    }
}
