using Application.OutBoundPort.IModdedTemplateRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class IsLinkedFromButtonAdapter(OkolarietDbContext db) : IIsLinkedFromButtonAsync
    {
        public Task<bool> IsLinkedFromButtonAsync(Guid moddedTemplateId, CancellationToken ct) =>
            db.ModdedTemplateItems.AnyAsync(i =>
                i.TargetModdedTemplateId == moddedTemplateId && i.ModdedTemplateId != moddedTemplateId, ct);
    }
}