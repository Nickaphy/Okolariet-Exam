using Domain.Enums;

namespace Domain.ValueObjects
{
    // Et snapshot er et fastfrosset billede af et projekts indhold på publiceringstidspunktet.
    // Det er det, kiosken afspiller, og det ændres aldrig efter oprettelsen.
    // Typerne herunder er records: rene dataobjekter uden id og uden adfærd.
    //
    // Sikkerhed: AnswerSnapshot indeholder med vilje IKKE oplysning om, hvorvidt et svar er rigtigt.
    // Serveren afgør selv, om et svar er korrekt. Så kan rigtige svar ikke læses ud af data,
    // der sendes til kiosken.

    // Hele projektet: det fælles udseende, id'et på startskærmen og alle skærmene.
    public record VersionSnapshot(Appearance Appearance, Guid MainModdedTemplateId, IReadOnlyList<ModdedTemplateSnapshot> ModdedTemplates);

    // Én skærm: skabelontype (afgør hvilken Blazor-komponent der tegner den), titel,
    // evt. eget udseende (null = brug projektets), udfyldte felter og evt. quizspørgsmål.
    public record ModdedTemplateSnapshot(Guid Id, string TemplateType, string? Title, Appearance? Appearance,
        IReadOnlyList<ItemSnapshot> Items, IReadOnlyList<QuestionSnapshot> Questions);

    // Ét udfyldt felt på en skærm. SlotKey identificerer feltet (fx "title").
    // Et felt indeholder tekst, en fil (AssetId + AssetKind) eller en knap, der fører videre
    // til en anden skærm (TargetModdedTemplateId).
    public record ItemSnapshot(string SlotKey, string? Text, Guid? AssetId, AssetKind? AssetKind, Guid? TargetModdedTemplateId);

    // Et quizspørgsmål med valgfrit medie og dets svarmuligheder.
    public record QuestionSnapshot(Guid Id, string Text, Guid? AssetId, AssetKind? AssetKind, IReadOnlyList<AnswerSnapshot> Answers);

    // Én svarmulighed. SortOrder bestemmer rækkefølgen, de vises i.
    public record AnswerSnapshot(Guid Id, string Text, Guid? AssetId, AssetKind? AssetKind, int SortOrder);
}