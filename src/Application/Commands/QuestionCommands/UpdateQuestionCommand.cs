namespace Application.Commands.QuestionCommands
{
    public record UpdateQuestionCommand(Guid QuestionId, string Text, Guid? AssetId, int SortOrder, string IfMatch);
}