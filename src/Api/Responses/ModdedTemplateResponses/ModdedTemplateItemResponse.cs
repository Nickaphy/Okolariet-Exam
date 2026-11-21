using Domain.Entities;   // ModdedTemplateItem

namespace Api.Responses.ModdedTemplateResponses
{
    //No Id: the UI identifies an item by its SlotKey.
    public record ModdedTemplateItemResponse(string SlotKey, string? Text, Guid? AssetId, Guid? TargetModdedTemplateId)
    {
        public static ModdedTemplateItemResponse From(ModdedTemplateItem i) => new(i.SlotKey, i.Text, i.AssetId, i.TargetModdedTemplateId);
    }
}