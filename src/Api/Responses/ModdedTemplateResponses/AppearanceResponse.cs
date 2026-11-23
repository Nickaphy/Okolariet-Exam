using Domain.ValueObjects;   // Appearance

namespace Api.Responses.ModdedTemplateResponses
{
    public record AppearanceResponse(string? BackgroundColor, string? ButtonColor, string? TextColor, string? FontFamily)
    {
        public static AppearanceResponse From(Appearance a) => new(a.BackgroundColor, a.ButtonColor, a.TextColor, a.FontFamily);
    }
}