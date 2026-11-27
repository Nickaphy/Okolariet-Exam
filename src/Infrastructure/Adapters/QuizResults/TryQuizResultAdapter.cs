using Application.OutBoundPort.IQuizResultRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.QuizResults
{
    public sealed class TryQuizResultAdapter(OkolarietDbContext db) : ITryAddQuizResultAsync
    {
        // The unique index on (SessionId, QuestionId) rejects a second answer for the same question
        // in the same session, including concurrent submits. The port reports that as false.
        public async Task<bool> TryAddAsync(QuizResult quizResult, CancellationToken ct)
        {
            db.QuizResults.Add(quizResult);
            try
            {
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return false;
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException sql && (sql.Number is 2601 or 2627);
    }
}
