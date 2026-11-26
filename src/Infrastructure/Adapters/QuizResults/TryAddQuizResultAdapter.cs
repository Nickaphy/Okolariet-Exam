using Application.OutBoundPort.IQuizResultRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.QuizResults
{
    public sealed class TryAddQuizResultAdapter(OkolarietDbContext db) : ITryAddQuizResultAsync
    {
        // The unique index (SessionId, QuestionId) decides. That also holds when two requests arrive at the same time.
        public async Task<bool> TryAddAsync(QuizResult quizResult, CancellationToken ct)
        {
            db.QuizResults.Add(quizResult);
            try
            {
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // 2601/2627 = duplicate key. Detach it, so the failed insert isn't retried on the next save.
                db.Entry(quizResult).State = EntityState.Detached;
                return false;
            }
        }
    }
}