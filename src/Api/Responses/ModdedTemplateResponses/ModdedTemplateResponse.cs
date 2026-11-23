using Api.Responses.QuestionResponses;   // QuestionResponse (yours, reused)
using Domain.Entities;                   // ModdedTemplate, Question

namespace Api.Responses.ModdedTemplateResponses
{
    // The "full" version for lists.
    // Questions is null unless they were fetched (only GET does that).
    public record ModdedTemplateResponse(
        Guid Id,
        Guid ProjectId,
        string TemplateType,
        string Title,
        AppearanceResponse Appearance,
        int SortOrder,
        IReadOnlyList<ModdedTemplateItemResponse> Items,
        IReadOnlyList<QuestionResponse>? Questions)
    {
        public static ModdedTemplateResponse From(ModdedTemplate m, IReadOnlyList<Question>? questions = null) => new(
            m.Id,
            m.ProjectId,
            m.TemplateType,
            m.Title,
            AppearanceResponse.From(m.Appearance),
            m.SortOrder,
            m.Items.Select(ModdedTemplateItemResponse.From).ToList(),
            questions?.Select(QuestionResponse.From).ToList());
    }
}