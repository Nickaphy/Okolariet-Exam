using Application.Models;
using Application.OutBoundPort.ISessionRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Sessions
{
    public sealed class GetSessionSummaryAdapter(OkolarietDbContext db) : IGetSessionSummaryAsync
    {
        public async Task<SessionSummary?> GetSummaryAsync(Guid sessionId, CancellationToken ct)
        {
            // The session gives us the project ID and the start time for the summary.
            var session = await db.Sessions.FindAsync([sessionId], ct);

            if (session is null)
                return null;

            // Retrieve the project's template ID to filter questions.
            var projectTemplateIds = db.ModdedTemplates
                .Where(template => template.ProjectId == session.ProjectId)
                .Select(template => template.Id);

            // Retrieve the project's active question IDs to filter results.
            var projectQuestionIds = db.Questions
                .Where(question => question.IsActive &&
                    projectTemplateIds.Contains(question.ModdedTemplateId))
                .Select(question => question.Id);

            // Retrieve the project's quiz results to calculate scores.
            var projectResults = db.QuizResults
                .Where(result => projectQuestionIds.Contains(result.QuestionId));

            // Retrieve the session's results to calculate scores.
            var sessionResults = projectResults
                .Where(result => result.SessionId == session.Id);


            var totalQuestions = await projectQuestionIds.CountAsync(ct); // Count the total number of active questions in the project.
            var answered = await sessionResults.CountAsync(ct); // Count the number of questions answered by the session.
            var correct = await sessionResults.CountAsync(result => result.IsCorrect, ct); // Count the number of correct answers.

            // Calculate the percentage of correct answers.
            var percentage = totalQuestions == 0 ? 0 : 100.0 * correct / totalQuestions;

            // Count every session in this project, including guests who have yet to answer.
            var projectSessionIds = db.Sessions
                .Where(guest => guest.ProjectId == session.ProjectId)
                .Select(guest => guest.Id);

            var guestCount = await projectSessionIds.CountAsync(ct); // Count the number of guest sessions in the project.
            var allGuestsCorrect = await projectResults.LongCountAsync( // Count the number of correct answers by guest sessions.
                result => result.IsCorrect && projectSessionIds.Contains(result.SessionId), ct); // Count the number of correct answers by guest sessions.

            // Divide all correct answers by the number of guests to get the average correct count.
            // Divide that count by the shared question total to get the average percentage.
            var allGuestsAveragePercentage = totalQuestions == 0 || guestCount == 0
                ? 0
                : 100.0 * allGuestsCorrect / guestCount / totalQuestions;

            // Pass the loaded and calculated values to the summary returned by the use case.
            return new SessionSummary(
                session.Id,
                session.StartedAt,
                totalQuestions,
                answered,
                correct,
                percentage,
                allGuestsAveragePercentage);
        }
    }
}
