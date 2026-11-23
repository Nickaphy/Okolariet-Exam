using FluentResults;

namespace Application.InBoundPort.IUserUsecases;

public interface IClearQuizLeaderboardUseCase
{
    // inbound port implemented by the application for clearing entries for the configures quiz.
    Task<Result> ClearAsync(
        Guid moddedTemplateId,
        CancellationToken ct);
}