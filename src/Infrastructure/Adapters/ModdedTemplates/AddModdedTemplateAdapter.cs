using Application.OutBoundPort.IModdedTemplateRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class AddModdedTemplateAdapter(OkolarietDbContext db) : IAddModdedTemplateAsync
    {
        public async Task AddModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct)
        {
            db.ModdedTemplates.Add(moddedTemplate);
            await db.SaveChangesAsync(ct);
        }
    }
}