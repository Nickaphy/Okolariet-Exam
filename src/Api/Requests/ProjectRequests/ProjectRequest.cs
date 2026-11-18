using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects;

namespace Api.Requests.ProjectRequests
{
    // Det, klienten sender ind ved oprettelse og opdatering (samme body, jf. ProjectInput i openapi.yaml).
    //
    // Attributterne i firkantede klammer er valideringskrav. Bliver et krav brudt, svarer API'et
    // automatisk HTTP 400, før use casen kører:
    // Required = feltet må ikke mangle
    // StringLength(120, MinimumLength = 1) = mellem 1 og 120 tegn
    // "[property: ...]" er den syntaks, der bruges til attributter på en records parametre.
    public record ProjectRequest(
        [property: Required, StringLength(120, MinimumLength = 1)] string Name,
        Guid? MainModdedTemplateId,        // valgfri
        AppearanceRequest? Appearance);    // valgfri

    // Udseendet, som klienten sender.
    public record AppearanceRequest(
        // RegularExpression kræver, at teksten matcher et mønster. "^#[0-9A-Fa-f]{6}$" betyder:
        // et #, efterfulgt af præcis 6 hex-tegn (0-9, A-F), og intet andet. Fx #0A6E8A er gyldig, mens
        // "rød" og "#FFF" ikke er. Er værdien null, springes kontrollen over.
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? BackgroundColor,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? ButtonColor,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$")] string? TextColor,
        [property: StringLength(100)] string? FontFamily)
    {
        // Konverterer til domænets Appearance, så resten af systemet ikke afhænger af Api's request-typer.
        public Appearance ToDomain() => new(BackgroundColor, ButtonColor, TextColor, FontFamily);
    }
}