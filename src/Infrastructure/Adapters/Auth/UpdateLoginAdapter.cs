using Application.OutBoundPort.AuthRepository.Logins;
using Domain.Entities.AuthEntities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Auth
{
    public sealed class UpdateLoginAdapter(OkolarietDbContext db) : IUpdateLoginAsync
    {
        public async Task UpdateLoginAsync(StaffLogin login, CancellationToken ct = default)
        {
            // Normally it came from GetLoginByIdAdapter and is tracked already; attach it if not.
            if (db.Entry(login).State == EntityState.Detached) db.StaffLogins.Update(login);
            await db.SaveChangesAsync(ct);
        }
    }
}
