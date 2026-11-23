namespace Application.OutBoundPort.IModdedTemplateRepository
{
    public interface IIsLinkedFromButtonAsync
    {
        // true if a button on another modded template leads here -> Delete must be blocked with 409
        Task<bool> IsLinkedFromButtonAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}