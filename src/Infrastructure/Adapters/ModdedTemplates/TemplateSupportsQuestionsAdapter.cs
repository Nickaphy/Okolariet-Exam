using Application.OutBoundPort.IModdedTemplateRepository;
using Infrastructure.Database;
using Infrastructure.Templates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.ModdedTemplates
{
    public sealed class TemplateSupportsQuestionsAdapter(OkolarietDbContext db) : ITemplateSupportsQuestionsAsync
    {
        public async Task<bool> SupportsQuestionsAsync(Guid moddedTemplateId, CancellationToken ct)
        {
            var templateType = await db.ModdedTemplates
                .Where(m => m.Id == moddedTemplateId)
                .Select(m => m.TemplateType)
                .FirstOrDefaultAsync(ct);

            return templateType is not null
                && TemplateCatalog.Find(templateType)?.SupportsQuestions() == true;
        }
    }
}