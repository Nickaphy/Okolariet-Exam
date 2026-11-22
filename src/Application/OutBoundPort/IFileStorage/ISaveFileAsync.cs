using Domain.Entities;

namespace Application.OutBoundPort.IFileStorage
{
    public interface ISaveFileAsync
    {
        Task<string> SaveFileAsync(Stream content, string contentType, CancellationToken ct);   // returns StoredName
    }
}