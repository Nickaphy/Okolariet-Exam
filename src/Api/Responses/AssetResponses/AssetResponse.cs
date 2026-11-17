using Domain.Entities;
using Domain.Enums;

namespace Api.Responses.AssetResponses
{
    public record AssetResponse(Guid Id, AssetKind Kind, string OriginalName, string ContentType, long SizeBytes, DateTimeOffset UploadedAt, string Url)
{
    public static AssetResponse From(Asset a) => new(a.Id, a.Kind, a.OriginalName, a.ContentType, a.SizeBytes, a.UploadedAt, $"/api/v1/assets/{a.Id}");
}
}