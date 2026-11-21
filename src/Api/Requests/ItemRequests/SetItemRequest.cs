using System.ComponentModel.DataAnnotations;   // [StringLength]

namespace Api.Requests.ItemRequests
{
    //All optional; which ones make sense depends on the slot kind.
    public record SetItemRequest(
        [property: StringLength(500)] string? Text,
        Guid? AssetId,
        Guid? TargetModdedTemplateId);
}