

namespace Application.OutBoundPort.IUserRepository;

public interface IClearQuizLeaderboardAsync
{
    // Delete all User entries belonging to this configured quiz.
    // Return false if the configured quiz does not exist.
    // Return true for an existing quiz, including an empty leaderboard.
    Task<bool> ClearAsync(
        Guid moddedTemplateId,
        CancellationToken ct);
}