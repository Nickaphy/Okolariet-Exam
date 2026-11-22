namespace Application.Commands.AssetCommands
{
    public record UploadAssetCommand(Stream Content, string OriginalName, string ContentType, long SizeBytes);
}