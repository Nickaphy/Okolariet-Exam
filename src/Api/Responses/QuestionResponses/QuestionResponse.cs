using Api.Responses.AnswerResponses;
using Domain.Entities;

namespace Api.Responses.QuestionResponses
{
    public record QuestionResponse(Guid Id, Guid ModdedTemplateId, string Text, Guid? AssetId, int SortOrder, IReadOnlyList<AnswerResponse> Answers)
    {
        public static QuestionResponse From(Question q) => new(q.Id, q.ModdedTemplateId, q.Text, q.AssetId, q.SortOrder,
            q.Answers.Where(a => a.IsActive).Select(AnswerResponse.From).ToList());
    }
}