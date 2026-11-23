namespace Application.Commands.ModdedTemplateCommands
{
    public record SetItemCommand(Guid ModdedTemplateId, string SlotKey, string? Text, Guid? AssetId, Guid? TargetModdedTemplateId);
}