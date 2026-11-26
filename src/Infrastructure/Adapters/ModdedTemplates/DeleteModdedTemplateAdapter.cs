using Application.OutBoundPort.IModdedTemplateRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class DeleteModdedTemplateAdapter(OkolarietDbContext db) : IDeleteModdedTemplateAsync
    {
        // Items, questions and answers go with it (cascade).
        public async Task DeleteModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct)
        {
            db.ModdedTemplates.Remove(moddedTemplate);
            await db.SaveChangesAsync(ct);
        }
    }
}