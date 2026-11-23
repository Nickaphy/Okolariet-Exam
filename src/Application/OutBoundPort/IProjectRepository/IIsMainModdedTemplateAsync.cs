namespace Application.OutBoundPort.IProjectRepository
{
    public interface IIsMainModdedTemplateAsync
    {
        // true if this is the project's start screen (Project.MainModdedTemplateId) -> Delete must be blocked with 409
        Task<bool> IsMainModdedTemplateAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}