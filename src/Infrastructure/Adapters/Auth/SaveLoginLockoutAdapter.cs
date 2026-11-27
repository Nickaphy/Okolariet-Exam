using Application.OutBoundPort.AuthRepository.Lockouts;
using Domain.Entities.AuthEntities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Auth
{
    // Insert or update, decided by whether a row for the e-mail exists.
    public sealed class SaveLoginLockoutAdapter(OkolarietDbContext db) : ISaveLoginLockoutAsync
    {
        public async Task SaveLoginLockoutAsync(LoginLockout lockout, CancellationToken ct = default)
        {
            var existing = await db.LoginLockouts.FindAsync(new object[] { lockout.Email }, ct);
            if (existing is null)
                db.LoginLockouts.Add(lockout);                               // first failed attempt for this e-mail
            else if (!ReferenceEquals(existing, lockout))
                db.Entry(existing).CurrentValues.SetValues(lockout);         // a different object for the same e-mail: copy its values
            // else: it is the tracked object itself, so EF already sees the changes.

            await db.SaveChangesAsync(ct);
        }
    }
}
