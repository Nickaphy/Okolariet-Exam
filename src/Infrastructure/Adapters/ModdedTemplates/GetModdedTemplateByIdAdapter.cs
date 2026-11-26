using Application.OutBoundPort.IModdedTemplateRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class GetModdedTemplateByIdAdapter(OkolarietDbContext db) : IGetModdedTemplateByIdAsync
    {
        // Tracked on purpose: SetItem, ClearItem, Update and Delete change it and save it in the same request.
        // Items are loaded through AutoInclude (see ModdedTemplateConfiguration).
        public Task<ModdedTemplate?> GetModdedTemplateByIdAsync(Guid moddedTemplateId, CancellationToken ct) =>
            db.ModdedTemplates.FirstOrDefaultAsync(m => m.Id == moddedTemplateId, ct);
    }
}