using Domain.Entities;   // ModdedTemplate

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IDeleteModdedTemplateAsync
    {
        Task DeleteModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct);
    }
}