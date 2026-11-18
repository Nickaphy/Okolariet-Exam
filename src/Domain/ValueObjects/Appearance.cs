namespace Domain.ValueObjects
{
    // Udseendet på en udstilling eller en enkelt skærm: baggrundsfarve, knapfarve, tekstfarve og skrifttype.
    //
    // Backend'en gemmer og videregiver kun disse værdier (Project -> snapshot -> kiosk).
    // Det er først kiosken, der læser dem og bruger dem til at tegne skærmene, fx som CSS.
    //
    // Det er en record, fordi to udseender med de samme værdier er ens (ingen identitet/id).
    // Alle felter er nullable: null betyder "ikke valgt", og kiosken bruger så sin egen standardværdi.
    // Farver angives som hex-koder, fx #0A6E8A. Formatet valideres i AppearanceRequest.
    public record Appearance(
        string? BackgroundColor = null,   // skærmens baggrundsfarve
        string? ButtonColor = null,       // knappernes farve
        string? TextColor = null,         // tekstens farve
        string? FontFamily = null);       // skrifttype, fx "Arial"
}