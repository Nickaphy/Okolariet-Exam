using Application.OutBoundPort.ISessionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Sessions
{
    public sealed class GetSessionByIdAdapter(OkolarietDbContext db) : IGetSessionByIdAsync
    {
        // Read-only: SubmitQuizResult and CreateUser only read the session (e.g. its ProjectId).
        public Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct) =>
            db.Sessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }
}