namespace Application.Commands.ModdedTemplateCommands
{
    public record UpdateModdedTemplateCommand(Guid ModdedTemplateId, string Title, AppearanceInput? Appearance, int SortOrder, string IfMatch);
}