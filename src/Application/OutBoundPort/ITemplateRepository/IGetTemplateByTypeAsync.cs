using Domain.Entities;   // Template

namespace Application.OutBoundPort.ITemplateRepository
{
    public interface IGetTemplateByTypeAsync
    {
        Task<Template?> GetTemplateByTypeAsync(string templateType, CancellationToken ct);   // null = doesn't exist -> 404
    }
}