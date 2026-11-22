using Domain.Entities;
using Domain.ValueObjects;

namespace Api.Responses.VersionResponses
{
    // Kort svar til historiklisten: versionsnummer og tidspunkt.
    // Felterne skal have samme navne som VersionSummary i Generated/Output.cs, ellers kan klienten ikke læse svaret.
    public record VersionSummaryResponse(int VersionNumber, DateTimeOffset PublishedAt)
    {
        public static VersionSummaryResponse From(PublishedVersion v) => new(v.VersionNumber, v.PublishedAt);
    }

    // Fuldt svar med hele projektets indhold. Bruges til både seneste version og forhåndsvisning.
    // Felterne skal have samme navne som PublishedVersion i Generated/Output.cs.
    //
    // PublishedAt er nullable, fordi en forhåndsvisning aldrig er publiceret og ikke har et tidspunkt.
    // Skærmene sendes som snapshot-records direkte, hvilket er sikkert, fordi de ikke indeholder
    // oplysning om rigtige svar.
    public record PublishedVersionResponse(Guid ProjectId, int VersionNumber, DateTimeOffset? PublishedAt,
        Appearance Appearance, Guid MainModdedTemplateId, IReadOnlyList<ModdedTemplateSnapshot> ModdedTemplates)
    {
        // Versionsnummer og tidspunkt gives som parametre, fordi en forhåndsvisning ikke er en gemt version
        // og derfor ikke har egne værdier (der bruges 0 og null).
        public static PublishedVersionResponse From(Guid projectId, int versionNumber, DateTimeOffset? publishedAt, VersionSnapshot s)
            => new(projectId, versionNumber, publishedAt, s.Appearance, s.MainModdedTemplateId, s.ModdedTemplates);
    }
}