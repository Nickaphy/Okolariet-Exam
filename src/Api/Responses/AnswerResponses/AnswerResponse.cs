using Domain.Entities;

namespace Api.Responses.AnswerResponses
{
    public record AnswerResponse(Guid Id, Guid QuestionId, string Text, Guid? AssetId, bool IsCorrect, int SortOrder)
    {
        public static AnswerResponse From(Answer a) => new(a.Id, a.QuestionId, a.Text, a.AssetId, a.IsCorrect, a.SortOrder);
    }
}