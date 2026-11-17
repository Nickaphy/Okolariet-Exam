namespace Application.Commands.AnswerCommands
{
    public record CreateAnswerCommand(Guid QuestionId, string Text, Guid? AssetId, bool IsCorrect, int SortOrder);
}