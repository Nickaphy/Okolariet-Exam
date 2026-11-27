using Application.OutBoundPort.AuthRepository.Logins;
using Domain.Entities.AuthEntities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Auth
{
    public sealed class GetLoginByIdAdapter(OkolarietDbContext db) : IGetLoginByIdAsync
    {
        // Tracked on purpose: logout and refresh set RevokedAt and pass it to UpdateLoginAdapter.
        public async Task<StaffLogin?> GetLoginByIdAsync(Guid id, CancellationToken ct = default) =>
            await db.StaffLogins.FindAsync(new object[] { id }, ct);
    }
}
