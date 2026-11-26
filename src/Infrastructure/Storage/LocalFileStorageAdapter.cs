using Application.OutBoundPort.IFileStorage;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage
{
    // Stores uploaded files on disk in FileStorageSettings.FullRootPath. One class behind all three ports.
    public sealed class LocalFileStorageAdapter(IOptions<FileStorageSettings> options)
        : ISaveFileAsync, IOpenFileAsync, IDeleteFileAsync
    {
        private readonly string root = options.Value.FullRootPath;

        public async Task<string> SaveFileAsync(Stream content, string contentType, CancellationToken ct)
        {
            // We pick the name ourselves, never the uploaded file's name.
            var storedName = $"{Guid.NewGuid():N}{ExtensionFor(contentType)}";
            await using var file = new FileStream(PathFor(storedName), FileMode.CreateNew, FileAccess.Write,
                                                  FileShare.None, 81920, useAsync: true);
            await content.CopyToAsync(file, ct);
            return storedName;
        }

        public Task<Stream?> OpenFileAsync(string storedName, CancellationToken ct)
        {
            var path = PathFor(storedName);
            if (!File.Exists(path))
                return Task.FromResult<Stream?>(null);

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteFileAsync(string storedName, CancellationToken ct)
        {
            File.Delete(PathFor(storedName));   // does nothing if the file is already gone
            return Task.CompletedTask;
        }

        // Path.GetFileName strips any folder part, so "../x" can never reach outside the root.
        private string PathFor(string storedName) => Path.Combine(root, Path.GetFileName(storedName));

        private static string ExtensionFor(string contentType) => contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            "video/x-matroska" => ".mkv",
            "audio/mpeg" => ".mp3",
            "audio/wav" => ".wav",
            "audio/mp4" => ".m4a",
            _ => ""
        };
    }
}