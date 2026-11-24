using Application.Helpers;
using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Projects
{
    public sealed class ListProjectsAdapter(OkolarietDbContext db) : IListProjectsAsync
    {
        public async Task<PagedList<Project>> ListAsync(int page, int pageSize, CancellationToken ct)
        {
            var total = await db.Projects.CountAsync(ct);
            var items = await db.Projects
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)   // tie-breaker, so pages never overlap when two projects share a name
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedList<Project>(items, page, pageSize, total);
        }
    }
}
