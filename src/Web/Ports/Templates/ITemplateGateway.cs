using Web.Api.Contracts;   // Template (the generated class from Output.cs)

namespace Web.Ports.Templates
{
    // Gateway contract for Templates.
    public interface ITemplateGateway
    {
        //Get list of all templates
        Task<IReadOnlyList<Template>> ListTemplates(CancellationToken cancellationToken = default);
        //Get specific TemplateType
        Task<Template> GetTemplate(string templateType, CancellationToken cancellationToken = default);
    }
}