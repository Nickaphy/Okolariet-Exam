using Application.OutBoundPort.IModdedTemplateRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class ListModdedTemplatesByProjectAdapter(OkolarietDbContext db) : IListModdedTemplatesByProjectAsync
    {
        public async Task<IReadOnlyList<ModdedTemplate>> ListModdedTemplatesByProjectAsync(Guid projectId, CancellationToken ct) =>
            await db.ModdedTemplates
                .AsNoTracking()
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);
    }
}