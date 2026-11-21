using Domain.Entities;   // Template

namespace Application.OutBoundPort.ITemplateRepository
{
    public interface IListTemplatesAsync
    {
        Task<IReadOnlyList<Template>> ListTemplatesAsync(CancellationToken ct);
    }
}