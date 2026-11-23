using Domain.Entities;   // ModdedTemplate

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IAddModdedTemplateAsync
    {
        Task AddModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct);
    }
}