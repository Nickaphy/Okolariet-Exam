using System.ComponentModel.DataAnnotations;           // [RegularExpression], [StringLength]
using Application.Commands.ModdedTemplateCommands;     // AppearanceInput

namespace Api.Requests.ModdedTemplateRequests
{
    // All fields optional.
    // The regex rejects bad colours with a 400 before they reach the use case.
    public record AppearanceRequest(
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? BackgroundColor,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? ButtonColor,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? TextColor,
        [property: StringLength(60, MinimumLength = 1)] string? FontFamily)
    {
        public AppearanceInput ToInput() => new(BackgroundColor, ButtonColor, TextColor, FontFamily);
    }
}