namespace Application.Commands.QuestionCommands
{
    public record CreateQuestionCommand(Guid ModdedTemplateId, string Text, Guid? AssetId, int SortOrder);
}