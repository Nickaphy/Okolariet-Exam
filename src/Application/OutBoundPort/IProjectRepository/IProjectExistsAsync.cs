namespace Application.OutBoundPort.IProjectRepository
{
    public interface IProjectExistsAsync
    {
        Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct);
    }
}