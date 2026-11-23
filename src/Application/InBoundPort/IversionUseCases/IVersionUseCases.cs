using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.InBoundPort.IVersionUseCases
{
    // Inbound ports: det, controlleren kan bede Application om. Hver metode svarer til ét endpoint.
    // Result<T> indeholder enten værdien eller en fejl, så forventede fejl (fx "findes ikke")
    // kan returneres uden exceptions.

    // Historik over versioner for et projekt.
    public interface IListVersionsUseCase { Task<Result<IReadOnlyList<PublishedVersion>>> ListAsync(Guid projectId, CancellationToken ct); }

    // Publicerer projektet og returnerer den nye version.
    public interface IPublishVersionUseCase { Task<Result<PublishedVersion>> PublishAsync(Guid projectId, CancellationToken ct); }

    // Den seneste publicerede version (det kiosken viser).
    public interface IGetLatestVersionUseCase { Task<Result<PublishedVersion>> GetLatestAsync(Guid projectId, CancellationToken ct); }

    // Forhåndsvisning af projektets nuværende indhold. Returnerer et VersionSnapshot og ikke en
    // PublishedVersion, fordi en forhåndsvisning aldrig gemmes og derfor ikke har et rigtigt versionsnummer.
    public interface IGetDraftVersionUseCase { Task<Result<VersionSnapshot>> GetDraftAsync(Guid projectId, CancellationToken ct); }
}