using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Versions;

namespace Web.Adapters.Versions
{
    // Implementerer IVersionGateway ved at kalde API'et gennem IVersionsApi (genereret af Refit).
    // IVersionsApi leveres via dependency injection (registreret med AddApi<IVersionsApi>() i Program.cs).
    //
    // Alle Refit-kald returnerer et IApiResponse, som både kan indeholde data og en fejl.
    // Unwrap() returnerer dataen, hvis kaldet lykkedes, og kaster ellers en GatewayException med statuskoden.
    public class VersionGateway(IVersionsApi api) : IVersionGateway
    {
        // API'et returnerer ICollection. ToList() konverterer til en liste, som matcher interfacet.
        public async Task<IReadOnlyList<VersionSummary>> ListVersions(Guid projectId, CancellationToken ct = default)
            => (await api.ListVersions(projectId, ct)).Unwrap().ToList();

        public async Task<VersionSummary> PublishVersion(Guid projectId, CancellationToken ct = default)
            => (await api.PublishVersion(projectId, ct)).Unwrap();

        // Refit-metoden kræver parameteren If-None-Match, som bruges til caching: klienten kan fortælle,
        // hvilken version den allerede har, og få 304 "uændret" tilbage. Her ønsker vi altid hele versionen,
        // så vi sender null, hvilket betyder at headeren udelades.
        public async Task<PublishedVersion> GetLatestVersion(Guid projectId, CancellationToken ct = default)
            => (await api.GetLatestVersion(projectId, null!, ct)).Unwrap();

        public async Task<PublishedVersion> GetDraftVersion(Guid projectId, CancellationToken ct = default)
            => (await api.GetDraftVersion(projectId, ct)).Unwrap();
    }
}