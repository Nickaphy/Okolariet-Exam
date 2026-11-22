using Domain.Entities;

namespace Application.OutBoundPort.IFileStorage
{
    public interface IOpenFileAsync
    {
        Task<Stream?> OpenFileAsync(string storedName, CancellationToken ct);
    }
}