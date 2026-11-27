using Application.OutBoundPort.AuthRepository.Logins;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Auth
{
    // Ends every active login of one staff member (used when their password changes).
    // Already revoked logins keep their original time.
    public sealed class RevokeAllLoginsForStaffAdapter(OkolarietDbContext db) : IRevokeAllLoginsForStaffAsync
    {
        public async Task RevokeAllLoginsForStaffAsync(Guid staffId, DateTimeOffset revokedAt, CancellationToken ct = default)
        {
            var active = await db.StaffLogins
                .Where(l => l.StaffId == staffId && l.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var login in active)
                login.RevokedAt = revokedAt;

            await db.SaveChangesAsync(ct);
        }
    }
}
