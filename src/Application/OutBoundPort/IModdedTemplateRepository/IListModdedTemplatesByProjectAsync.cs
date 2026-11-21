using Domain.Entities;   // ModdedTemplate

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IListModdedTemplatesByProjectAsync
    {
        Task<IReadOnlyList<ModdedTemplate>> ListModdedTemplatesByProjectAsync(Guid projectId, CancellationToken ct);
    }
}