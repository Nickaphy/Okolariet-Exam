using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Projects
{
    public sealed class GetProjectByIdAdapter(OkolarietDbContext db) : IGetProjectByIdAsync
    {
        // Tracked on purpose: Update, Delete and Publish change the returned project and save it in the same request.
        public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct) =>
            db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}
