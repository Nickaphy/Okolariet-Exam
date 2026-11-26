using Application.OutBoundPort.ISessionRepository;
using Domain.Entities;
using Infrastructure.Database;

// Adapter implementing the ICreateSessionAsync port
namespace Infrastructure.Adapters.Sessions
{
    public sealed class CreateSessionAdapter(OkolarietDbContext db) : ICreateSessionAsync
    {
        public async Task CreateSessionAsync(Session session, CancellationToken ct)
        {
            db.Sessions.Add(session); // Marks the session as new entity in EF Core's change tracker.
            await db.SaveChangesAsync(ct); // async database operation, EF Core sees new session, sends INSERT to SQL server and waits for completion
        }
    }
}
