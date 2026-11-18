using Domain.Entities;

namespace Application.OutBoundPort.IFileStorage
{
    public interface IDeleteFileAsync
    {
        Task DeleteFileAsync(string storedName, CancellationToken ct);
    }
}