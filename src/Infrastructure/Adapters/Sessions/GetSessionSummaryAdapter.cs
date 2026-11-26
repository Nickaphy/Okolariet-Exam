using Application.Models;
using Application.OutBoundPort.ISessionRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Sessions
{
    // Read model for GET /sessions/{id}: the guest's progress plus a comparison with all guests in the project.
    public sealed class GetSessionSummaryAdapter(OkolarietDbContext db) : IGetSessionSummaryAsync
    {
        public async Task<SessionSummary?> GetSummaryAsync(Guid sessionId, CancellationToken ct)
        {
            var session = await db.Sessions
                .AsNoTracking()
                .Where(s => s.Id == sessionId)
                .Select(s => new { s.Id, s.ProjectId, s.StartedAt })
                .FirstOrDefaultAsync(ct);

            if (session is null)
                return null;   // -> 404 in the use case

            // The X in "7 of X": active questions on all of the project's modded templates.
            var totalQuestions = await db.Questions
                .Where(q => q.IsActive
                         && db.ModdedTemplates.Any(m => m.Id == q.ModdedTemplateId && m.ProjectId == session.ProjectId))
                .CountAsync(ct);

            // This guest's results.
            var answered = await db.QuizResults.CountAsync(r => r.SessionId == sessionId, ct);
            var correct = await db.QuizResults.CountAsync(r => r.SessionId == sessionId && r.IsCorrect, ct);

            // Correct answers per guest in the same project. Only guests who answered at least
            // one question are counted, so people who never played don't pull the average down.
            var averageCorrect = await db.QuizResults
                .Where(r => db.Sessions.Any(s => s.Id == r.SessionId && s.ProjectId == session.ProjectId))
                .GroupBy(r => r.SessionId)
                .Select(g => (double?)g.Count(r => r.IsCorrect))
                .AverageAsync(ct);   // null when nobody has answered yet

            return new SessionSummary(
                session.Id,
                session.StartedAt,
                totalQuestions,
                answered,
                correct,
                Percent(correct, totalQuestions),
                Percent(averageCorrect ?? 0, totalQuestions));
        }

        // Avoids dividing by zero when the project has no questions yet. Rounded to one decimal, like 62.4.
        private static double Percent(double correct, int total) =>
            total == 0 ? 0 : Math.Round(correct / total * 100, 1);
    }
}