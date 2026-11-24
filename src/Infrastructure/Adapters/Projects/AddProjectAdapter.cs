using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Projects
{
    public sealed class AddProjectAdapter(OkolarietDbContext db) : IAddProjectAsync
    {
        public async Task AddAsync(Project project, CancellationToken ct)
        {
            db.Projects.Add(project);
            await db.SaveChangesAsync(ct);
        }
    }
}
