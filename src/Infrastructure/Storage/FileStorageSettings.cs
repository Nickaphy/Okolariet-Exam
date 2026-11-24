namespace Infrastructure.Storage
{
    // Where uploaded files are stored on disk. Bound from the "FileStorage" configuration section.
    // The file storage adapter (ISaveFileAsync, IOpenFileAsync, IDeleteFileAsync) is owned by the Asset feature.
    // It should inject IOptions<FileStorageSettings> and always use FullRootPath, so every part of the system
    // agrees on the folder.
    public sealed class FileStorageSettings
    {
        // Relative paths are resolved from the process's working directory (the Host folder when run locally).
        // In Docker it is set to /app/uploads, which is backed by a named volume.
        public string RootPath { get; set; } = "uploads";

        public string FullRootPath => Path.GetFullPath(RootPath);
    }
}