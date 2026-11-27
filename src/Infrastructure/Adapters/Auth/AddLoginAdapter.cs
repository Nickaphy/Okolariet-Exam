using Application.OutBoundPort.AuthRepository.Logins;
using Domain.Entities.AuthEntities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Auth
{
    public sealed class AddLoginAdapter(OkolarietDbContext db) : IAddLoginAsync
    {
        public async Task AddLoginAsync(StaffLogin login, CancellationToken ct = default)
        {
            db.StaffLogins.Add(login);
            await db.SaveChangesAsync(ct);
        }
    }
}
