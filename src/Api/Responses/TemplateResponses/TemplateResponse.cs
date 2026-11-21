using Domain.Entities;   // Template
using Domain.Enums;      // TemplateCategory

namespace Api.Responses.TemplateResponses
{
    public record TemplateResponse(
        string Type,
        string DisplayName,
        TemplateCategory Category,
        IReadOnlyList<SlotDefinitionResponse> Slots,
        IReadOnlyList<AppearanceFieldDefinitionResponse> AppearanceFields,
        QuestionRulesResponse? QuestionRules)
    {
        public static TemplateResponse From(Template t) => new(
            t.Type,
            t.DisplayName,
            t.Category,
            t.Slots.Select(SlotDefinitionResponse.From).ToList(),
            t.AppearanceFields.Select(AppearanceFieldDefinitionResponse.From).ToList(),
            t.QuestionRules is null ? null : QuestionRulesResponse.From(t.QuestionRules));   // only quizzes have rules
    }
}