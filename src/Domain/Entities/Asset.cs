using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    //Here are the datafiles that can be used for the templates
    public class Asset : Aggregateroot
    {
        public AssetKind Kind { get; private set; }
        public string OriginalName { get; private set; } = "";
        public string StoredName { get; private set; } = "";   // the file name on disk, never shown to the user
        public string ContentType { get; private set; } = "";
        public long SizeBytes { get; private set; }
        public DateTimeOffset UploadedAt { get; private set; }

        private Asset() { } // EF Core

        public static long MaxBytesFor(AssetKind kind) => kind switch
        {
            AssetKind.Image => 10_000_000,   // limits come from Output.cs
            AssetKind.Audio => 50_000_000,
            AssetKind.Video => 500_000_000,
            _ => 0
        };

        // Which content types are allowed, and what kind they become. Comes from the Accept header on GetAsset in Output.cs.
        // Returns null if the type isn't allowed -> the use case turns that into an error.
        public static AssetKind? KindFromContentType(string contentType) => contentType switch
        {
            "image/png" or "image/jpeg" or "image/webp" or "image/gif" => AssetKind.Image,
            "video/mp4" or "video/webm" or "video/x-matroska"          => AssetKind.Video,
            "audio/mpeg" or "audio/wav" or "audio/mp4"                 => AssetKind.Audio,
            _ => null
        };

        public static Asset Create(AssetKind kind, string originalName, string storedName,
                                string contentType, long sizeBytes, DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(originalName)) throw new ArgumentException("Name required.", nameof(originalName));
            if (sizeBytes <= 0) throw new ArgumentException("File is empty.", nameof(sizeBytes));
            return new Asset { Kind = kind, OriginalName = originalName, StoredName = storedName,
                            ContentType = contentType, SizeBytes = sizeBytes, UploadedAt = now };
        }
    }
}