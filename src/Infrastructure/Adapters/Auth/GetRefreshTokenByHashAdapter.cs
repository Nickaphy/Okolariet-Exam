using Application.OutBoundPort.AuthRepository.RefreshToken;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Domain.Entities.AuthEntities.RefreshToken;   // the port's namespace is also called RefreshToken

namespace Infrastructure.Adapters.Auth
{
    public sealed class GetRefreshTokenByHashAdapter(OkolarietDbContext db) : IGetRefreshTokenByHashAsync
    {
        // Only the hash is stored, so the lookup is by hash (unique index, see RefreshTokenConfiguration).
        // Tracked on purpose: RefreshTokensUseCase sets UsedAt and passes it to UpdateRefreshTokenAdapter.
        public Task<RefreshTokenEntity?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) =>
            db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }
}
