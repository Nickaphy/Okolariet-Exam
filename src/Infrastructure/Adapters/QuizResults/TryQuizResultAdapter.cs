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
            db.QuizResults.Add(quizResult); // add the quiz result to the database
            try
            {
                await db.SaveChangesAsync(ct); // save the changes to the database
                return true; // return true if the quiz result was added successfully
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) // if the quiz result is a unique violation
            {
                return false; // return false if the quiz result is a unique violation
            }
        }

        // Dupelicate protection, if the quiz result is a unique violation, return false
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql && (sql.Number is 2601 or 2627);
        }
    } 
}
