using Application.ErrorMessages;
using Application.InBoundPort.IUserUsecases;
using Application.OutBoundPort.IUserRepository;
using FluentResults;

namespace Application.UseCases.Users;

public class ClearQuizLeaderboardUseCase(
    IClearQuizLeaderboardAsync clearLeaderboard)
    : IClearQuizLeaderboardUseCase
{
    public async Task<Result> ClearAsync(
        Guid moddedTemplateId,
        CancellationToken ct)
    {
        // Infrastructure checks existence and clears the saved entries.
        var quizExists = await clearLeaderboard.ClearAsync(
            moddedTemplateId, ct);

        if (!quizExists)
            return Result.Fail(
                new NotFoundError("Quiz not found."));

        return Result.Ok();
    }
}