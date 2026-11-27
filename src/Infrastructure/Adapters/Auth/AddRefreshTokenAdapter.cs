using Application.OutBoundPort.AuthRepository.RefreshToken;
using Infrastructure.Database;
using RefreshTokenEntity = Domain.Entities.AuthEntities.RefreshToken;   // the port's namespace is also called RefreshToken

namespace Infrastructure.Adapters.Auth
{
    public sealed class AddRefreshTokenAdapter(OkolarietDbContext db) : IAddRefreshTokenAsync
    {
        public async Task AddRefreshTokenAsync(RefreshTokenEntity token, CancellationToken ct = default)
        {
            db.RefreshTokens.Add(token);
            await db.SaveChangesAsync(ct);
        }
    }
}
