using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Projects
{
    public sealed class DeleteProjectAdapter(OkolarietDbContext db) : IDeleteProjectAsync
    {
        // The database cascades the delete to the project's screens, questions, versions, sessions and results.
        // Assets are shared between projects and are left alone.
        public async Task DeleteAsync(Project project, CancellationToken ct)
        {
            db.Projects.Remove(project);
            await db.SaveChangesAsync(ct);
        }
    }
}
