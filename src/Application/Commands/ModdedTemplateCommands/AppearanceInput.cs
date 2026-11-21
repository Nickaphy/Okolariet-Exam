using Domain.ValueObjects;   // Appearance

namespace Application.Commands.ModdedTemplateCommands
{
    // The raw colours/font from the user. Turned into a domain Appearance INSIDE the use case,
    // so validation errors are caught by its try/catch and become 422, not a 500 in the controller.
    public record AppearanceInput(string? BackgroundColor, string? ButtonColor, string? TextColor, string? FontFamily)
    {
        public Appearance ToDomain() => new(BackgroundColor, ButtonColor, TextColor, FontFamily);
    }
}