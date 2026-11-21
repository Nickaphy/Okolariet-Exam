using Domain.Entities;   // ModdedTemplate

namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IGetModdedTemplateByIdAsync
    {
        Task<ModdedTemplate?> GetModdedTemplateByIdAsync(Guid moddedTemplateId, CancellationToken ct);   
        // includes its Items
    }
}