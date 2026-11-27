using Application.OutBoundPort.AuthRepository.Lockouts;
using Domain.Entities.AuthEntities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Auth
{
    public sealed class GetLoginLockoutAdapter(OkolarietDbContext db) : IGetLoginLockoutAsync
    {
        // Tracked on purpose: LoginUseCase changes the counter and passes it to SaveLoginLockoutAdapter.
        // FindAsync looks the e-mail (the primary key) up, also among entities the context already tracks.
        public async Task<LoginLockout?> GetLoginLockoutAsync(string normalizedEmail, CancellationToken ct = default) =>
            await db.LoginLockouts.FindAsync(new object[] { normalizedEmail }, ct);
    }
}
