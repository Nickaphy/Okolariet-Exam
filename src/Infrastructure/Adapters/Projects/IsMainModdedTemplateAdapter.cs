using Application.OutBoundPort.IProjectRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Projects
{
    public sealed class IsMainModdedTemplateAdapter(OkolarietDbContext db) : IIsMainModdedTemplateAsync
    {
        public Task<bool> IsMainModdedTemplateAsync(Guid moddedTemplateId, CancellationToken ct) =>
            db.Projects.AnyAsync(p => p.MainModdedTemplateId == moddedTemplateId, ct);
    }
}
