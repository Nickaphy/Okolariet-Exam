using Domain.Entities;   // ModdedTemplate, Question

namespace Application.Helpers
{
    // GetModdedTemplate returns "a modded template with its items and questions".
    // Questions are their own aggregate, so they're fetched separately and bundled here.
    public record ModdedTemplateWithQuestions(ModdedTemplate ModdedTemplate, IReadOnlyList<Question> Questions);
}