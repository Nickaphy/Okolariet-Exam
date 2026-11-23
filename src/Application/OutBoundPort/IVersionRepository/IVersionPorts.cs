using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.OutBoundPort.IVersionRepository
{
    // Outbound ports: det, use casene har brug for fra omverdenen (database m.m.).
    // Application definerer interfaces her, og Infrastructure implementerer dem senere.
    // Dermed afhænger forretningslogikken ikke af, hvordan data konkret gemmes.

    // Gemmer en ny version.
    public interface IAddVersionAsync { Task AddAsync(PublishedVersion version, CancellationToken ct); }

    // Henter alle versioner for et projekt, nyeste først.
    public interface IListVersionsAsync { Task<IReadOnlyList<PublishedVersion>> ListAsync(Guid projectId, CancellationToken ct); }

    // Henter den nyeste version. Returnerer null, hvis projektet aldrig er publiceret.
    public interface IGetLatestVersionAsync { Task<PublishedVersion?> GetLatestAsync(Guid projectId, CancellationToken ct); }

    // Bygger et snapshot ud fra projektets nuværende indhold (skærme, felter, spørgsmål og svar),
    // uden oplysning om hvilke svar der er rigtige.
    // Returnerer et fejlet Result (BusinessRuleError, HTTP 422), hvis projektet ikke kan vises eller
    // publiceres, fx fordi der ikke er valgt en startskærm, eller et quizspørgsmål ikke har præcis ét rigtigt svar.
    // Afhænger af ModdedTemplate, Question og Answer. Implementeres af den, der laver dem, i Infrastructure.
    public interface IBuildSnapshotAsync { Task<Result<VersionSnapshot>> BuildAsync(Guid projectId, CancellationToken ct); }
}