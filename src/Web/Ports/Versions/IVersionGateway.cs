using Web.Api.Contracts;

namespace Web.Ports.Versions
{
    // Kontrakten mellem Blazor-siderne og API'et for versioner. Siderne kender kun dette interface
    // og ved ikke noget om HTTP eller Refit.
    // Metoderne svarer til IVersionsApi i Generated/Output.cs, men uden IApiResponse (fejl håndteres i adapteren),
    // og ICollection er erstattet af IReadOnlyList.
    public interface IVersionGateway
    {
        // Historik over alle versioner af et projekt. VersionSummary indeholder kun versionsnummer og
        // tidspunkt, ikke selve indholdet. Bruges til en oversigt i editoren.
        Task<IReadOnlyList<VersionSummary>> ListVersions(Guid projectId, CancellationToken ct = default);

        // Publicerer projektet: API'et opretter en ny version ud fra projektets nuværende indhold.
        // Svarer med den nye versions nummer og tidspunkt.
        Task<VersionSummary> PublishVersion(Guid projectId, CancellationToken ct = default);

        // Henter den seneste publicerede version med alt indhold. Det er det kiosken viser.
        Task<PublishedVersion> GetLatestVersion(Guid projectId, CancellationToken ct = default);

        // Henter en forhåndsvisning af projektet, som det ser ud lige nu, uden at publicere.
        // Bruges af personalet til at kontrollere resultatet. Der gemmes ikke noget.
        Task<PublishedVersion> GetDraftVersion(Guid projectId, CancellationToken ct = default);
    }
}