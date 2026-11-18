namespace Application.Commands.AnswerCommands
{
    public record UpdateAnswerCommand(Guid AnswerId, string Text, Guid? AssetId, bool IsCorrect, int SortOrder);
}