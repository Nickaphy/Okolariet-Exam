using Application.OutBoundPort.IQuizResultRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.QuizResults
{
    public sealed class ListQuizResultsBySessionAdapter(OkolarietDbContext db) : IListQuizResultsBySessionAsync
    {
        public async Task<IReadOnlyList<QuizResult>> ListBySessionAsync(Guid sessionId, CancellationToken ct) =>
            await db.QuizResults
                .AsNoTracking()
                .Where(r => r.SessionId == sessionId)
                .ToListAsync(ct);
    }
}