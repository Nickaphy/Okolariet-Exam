using Application.OutBoundPort.IQuizResultRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.QuizResults
{
    public sealed class ListQuizResultsBySessionAdapter(OkolarietDbContext db) : IListQuizResultsBySessionAsync
    {
        public async Task<IReadOnlyList<QuizResult>> ListBySessionAsync(Guid sessionId, CancellationToken ct)
        {
            return await db.QuizResults
                .AsNoTracking() // no tracking to avoid stale data
                .Where(qr => qr.SessionId == sessionId) // where the quiz result session id is the same as the session id
                .ToListAsync(ct); // return the list of quiz results
        }
    }
}
