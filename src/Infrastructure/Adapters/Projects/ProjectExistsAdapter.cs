using Application.OutBoundPort.IProjectRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Projects
{
    public sealed class ProjectExistsAdapter(OkolarietDbContext db) : IProjectExistsAsync
    {
        public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct) =>
            db.Projects.AnyAsync(p => p.Id == projectId, ct);
    }
}
