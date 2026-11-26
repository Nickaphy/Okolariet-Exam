using Application.OutBoundPort.ITemplateRepository;
using Domain.Entities;
using Infrastructure.Templates;

namespace Infrastructure.Adapters.Templates
{
    public sealed class TemplateCatalogAdapter : IGetTemplateByTypeAsync, IListTemplatesAsync
    {
        public Task<Template?> GetTemplateByTypeAsync(string templateType, CancellationToken ct) =>
            Task.FromResult(TemplateCatalog.Find(templateType));

        public Task<IReadOnlyList<Template>> ListTemplatesAsync(CancellationToken ct) =>
            Task.FromResult(TemplateCatalog.All);
    }
}