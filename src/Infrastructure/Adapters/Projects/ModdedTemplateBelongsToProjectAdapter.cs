using Application.OutBoundPort.IProjectRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Projects
{
    public sealed class ModdedTemplateBelongsToProjectAdapter(OkolarietDbContext db) : IModdedTemplateBelongsToProjectAsync
    {
        public Task<bool> BelongsToProjectAsync(Guid moddedTemplateId, Guid projectId, CancellationToken ct) =>
            db.ModdedTemplates.AnyAsync(m => m.Id == moddedTemplateId && m.ProjectId == projectId, ct);
    }
}
