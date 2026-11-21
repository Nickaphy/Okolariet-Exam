namespace Application.Commands.ModdedTemplateCommands
{
    public record CreateModdedTemplateCommand(Guid ProjectId, string TemplateType, string Title, AppearanceInput? Appearance, int SortOrder);
}