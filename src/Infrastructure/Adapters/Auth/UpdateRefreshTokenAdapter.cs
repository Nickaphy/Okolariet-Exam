using Application.OutBoundPort.AuthRepository.RefreshToken;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Domain.Entities.AuthEntities.RefreshToken;   // the port's namespace is also called RefreshToken

namespace Infrastructure.Adapters.Auth
{
    public sealed class UpdateRefreshTokenAdapter(OkolarietDbContext db) : IUpdateRefreshTokenAsync
    {
        public async Task UpdateRefreshTokenAsync(RefreshTokenEntity token, CancellationToken ct = default)
        {
            if (db.Entry(token).State == EntityState.Detached) db.RefreshTokens.Update(token);
            await db.SaveChangesAsync(ct);
        }
    }
}
