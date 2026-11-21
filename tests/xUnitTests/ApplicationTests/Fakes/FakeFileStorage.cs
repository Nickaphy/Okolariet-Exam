using Application.OutBoundPort.IFileStorage;   // ISaveFileAsync, IOpenFileAsync, IDeleteFileAsync

namespace xUnitTests.ApplicationTests.Fakes
{
    // Pretends to be the disk: files live in a dictionary instead.
    public class FakeFileStorage : ISaveFileAsync, IOpenFileAsync, IDeleteFileAsync
    {
        public Dictionary<string, byte[]> Files { get; } = new();

        public async Task<string> SaveFileAsync(Stream content, string contentType, CancellationToken ct)
        {
            using var memory = new MemoryStream();
            await content.CopyToAsync(memory, ct);
            var storedName = Guid.NewGuid().ToString("N");
            Files[storedName] = memory.ToArray();
            return storedName;
        }

        public Task<Stream?> OpenFileAsync(string storedName, CancellationToken ct)
            => Task.FromResult<Stream?>(Files.TryGetValue(storedName, out var bytes) ? new MemoryStream(bytes) : null);

        public Task DeleteFileAsync(string storedName, CancellationToken ct)
        {
            Files.Remove(storedName);
            return Task.CompletedTask;
        }
    }
}
