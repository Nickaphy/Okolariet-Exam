using Application.OutBoundPort.IModdedTemplateRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class UpdateModdedTemplateAdapter(OkolarietDbContext db) : IUpdateModdedTemplateAsync
    {
        // NO db.Update() here. The modded template is already tracked (from GetModdedTemplateByIdAdapter).
        // New items already have an Id (Guid.NewGuid in Entity), so Update() would mark them as Modified
        // and EF would send an UPDATE for a row that doesn't exist. SaveChanges' change detection instead
        // inserts new items, updates changed ones and deletes the ones removed by ClearItem.
        public Task UpdateModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct) =>
            db.SaveChangesAsync(ct);
    }
}