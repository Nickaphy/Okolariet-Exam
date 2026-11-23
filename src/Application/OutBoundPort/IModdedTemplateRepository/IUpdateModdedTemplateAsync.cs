using Domain.Entities;   // ModdedTemplate

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IUpdateModdedTemplateAsync
    {
        Task UpdateModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct);   
        // also saves new/changed/removed items
    }
}