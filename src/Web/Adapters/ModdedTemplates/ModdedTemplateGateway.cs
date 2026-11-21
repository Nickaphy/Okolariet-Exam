using Web.Api;                     // IModdedTemplatesApi
using Web.Api.Contracts;           // ModdedTemplate, ModdedTemplateSummary, ...
using Web.Helpers;                 // .Unwrap(), .EnsureSuccess()
using Web.Ports.ModdedTemplates;   // IModdedTemplateGateway

namespace Web.Adapters.ModdedTemplates
{
    public class ModdedTemplateGateway(IModdedTemplatesApi api) : IModdedTemplateGateway
    {
        public async Task<IReadOnlyList<ModdedTemplateSummary>> ListModdedTemplates(Guid projectId, CancellationToken cancellationToken = default)
            => (await api.ListModdedTemplates(projectId, cancellationToken)).Unwrap().ToList();

        public async Task<ModdedTemplate> CreateModdedTemplate(Guid projectId, ModdedTemplateCreate body, CancellationToken cancellationToken = default)
            => (await api.CreateModdedTemplate(projectId, body, cancellationToken)).Unwrap();

        public async Task<ModdedTemplate> GetModdedTemplate(Guid moddedTemplateId, CancellationToken cancellationToken = default)
            => (await api.GetModdedTemplate(moddedTemplateId, cancellationToken)).Unwrap();

        public async Task<ModdedTemplate> UpdateModdedTemplate(Guid moddedTemplateId, ModdedTemplateUpdate body, string ifMatch, CancellationToken cancellationToken = default)
            => (await api.UpdateModdedTemplate(moddedTemplateId, body, ifMatch, cancellationToken)).Unwrap();

        public async Task DeleteModdedTemplate(Guid moddedTemplateId, CancellationToken cancellationToken = default)
            => (await api.DeleteModdedTemplate(moddedTemplateId, cancellationToken)).EnsureSuccess();
    }
}