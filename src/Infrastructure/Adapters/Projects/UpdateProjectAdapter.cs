using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Projects
{
    public sealed class UpdateProjectAdapter(OkolarietDbContext db) : IUpdateProjectAsync
    {
        public async Task UpdateAsync(Project project, CancellationToken ct)
        {
            db.Projects.Update(project);
            await db.SaveChangesAsync(ct);
        }
    }
}
