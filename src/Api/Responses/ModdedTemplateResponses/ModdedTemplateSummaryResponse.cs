using Domain.Entities;   // ModdedTemplate

namespace Api.Responses.ModdedTemplateResponses
{
    // The "light" version for lists: no items, no questions, no appearance.
    public record ModdedTemplateSummaryResponse(Guid Id, Guid ProjectId, string TemplateType, string Title, int SortOrder)
    {
        public static ModdedTemplateSummaryResponse From(ModdedTemplate m) => new(m.Id, m.ProjectId, m.TemplateType, m.Title, m.SortOrder);
    }
}