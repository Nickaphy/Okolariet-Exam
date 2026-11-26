using Application.OutBoundPort.ISessionRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Sessions
{
    public sealed class CreateSessionAdapter(OkolarietDbContext db) : ICreateSessionAsync
    {
        public async Task CreateSessionAsync(Session session, CancellationToken ct)
        {
            db.Sessions.Add(session);
            await db.SaveChangesAsync(ct);
        }
    }
}