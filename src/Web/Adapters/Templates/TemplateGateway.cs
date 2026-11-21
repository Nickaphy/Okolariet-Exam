using Web.Api;                 // ITemplatesApi (the Refit interface)
using Web.Api.Contracts;       // Template
using Web.Helpers;             // .Unwrap()
using Web.Ports.Templates;     // ITemplateGateway

namespace Web.Adapters.Templates
{
    // Both endpoints return something, so both use .Unwrap(). 
    // Since we don't want the PO to accidentally delete a template (only modded),
    // we don't include a delete function here.
    public class TemplateGateway(ITemplatesApi api) : ITemplateGateway
    {
        public async Task<IReadOnlyList<Template>> ListTemplates(CancellationToken cancellationToken = default)
            => (await api.ListTemplates(cancellationToken)).Unwrap().ToList();

        public async Task<Template> GetTemplate(string templateType, CancellationToken cancellationToken = default)
            => (await api.GetTemplate(templateType, cancellationToken)).Unwrap();
    }
}