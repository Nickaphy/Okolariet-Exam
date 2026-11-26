using Domain.Entities;
using Infrastructure.Database;
using Application.OutBoundPort.ISessionRepository;

namespace Infrastructure.Adapters.Sessions
{
    public sealed class GetSessionByIdAdapter(OkolarietDbContext db) : IGetSessionByIdAsync
    {
        public async Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct)
        {
            // EF Core FindAsync retrieves the session by its primary key (sessionId)
            // If the session is tracked by the context, it will be returned immediately without a database query,
            // otherwise it will be retrieved from the database
            // It will be returned asynchronously back to the caller
            return await db.Sessions.FindAsync([sessionId], ct);
        }
    }
}
