using Web.Api.Contracts;   // ModdedTemplate, ModdedTemplateSummary, ModdedTemplateCreate, ModdedTemplateUpdate

namespace Web.Ports.ModdedTemplates
{
    // Gateway contract for ModdedTemplates.
    public interface IModdedTemplateGateway
    {
        Task<IReadOnlyList<ModdedTemplateSummary>> ListModdedTemplates(Guid projectId, CancellationToken cancellationToken = default);
        Task<ModdedTemplate> CreateModdedTemplate(Guid projectId, ModdedTemplateCreate body, CancellationToken cancellationToken = default);
        Task<ModdedTemplate> GetModdedTemplate(Guid moddedTemplateId, CancellationToken cancellationToken = default);
        Task<ModdedTemplate> UpdateModdedTemplate(Guid moddedTemplateId, ModdedTemplateUpdate body, string ifMatch, CancellationToken cancellationToken = default);
        Task DeleteModdedTemplate(Guid moddedTemplateId, CancellationToken cancellationToken = default);
    }
}