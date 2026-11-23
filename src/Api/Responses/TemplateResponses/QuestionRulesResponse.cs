using Domain.ValueObjects;   // QuestionRules

namespace Api.Responses.TemplateResponses
{
    public record QuestionRulesResponse(int MaxQuestions, int AnswersPerQuestion)
    {
        public static QuestionRulesResponse From(QuestionRules q) => new(q.MaxQuestions, q.AnswersPerQuestion);
    }
}