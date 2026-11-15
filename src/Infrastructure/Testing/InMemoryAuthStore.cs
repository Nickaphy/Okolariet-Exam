using System.Collections.Concurrent;
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Domain.Entities.AuthEntities;
using Domain.ValueObjects;

namespace Infrastructure.Testing
{
    // TEMPORARY until the auth tables exist. All data is lost when the Host restarts.
    public sealed class InMemoryAuthStore :
        IGetLoginLockoutAsync, ISaveLoginLockoutAsync,
        IAddLoginAsync, IGetLoginByIdAsync, IUpdateLoginAsync, IRevokeAllLoginsForStaffAsync,
        IAddRefreshTokenAsync, IGetRefreshTokenByHashAsync, IUpdateRefreshTokenAsync
    {
        private readonly ConcurrentDictionary<string, LoginLockout> _lockouts = new();
        private readonly ConcurrentDictionary<Guid, StaffLogin> _logins = new();
        private readonly ConcurrentDictionary<string, RefreshToken> _refreshTokens = new();   // key: TokenHash

        public Task<LoginLockout?> GetLoginLockoutAsync(string normalizedEmail, CancellationToken ct = default) =>
            Task.FromResult<LoginLockout?>(_lockouts.TryGetValue(normalizedEmail, out var l) ? l : null);

        public Task SaveLoginLockoutAsync(LoginLockout lockout, CancellationToken ct = default)
        {
            _lockouts[lockout.Email] = lockout;
            return Task.CompletedTask;
        }

        public Task AddLoginAsync(StaffLogin login, CancellationToken ct = default)
        {
            _logins[login.Id] = login;
            return Task.CompletedTask;
        }

        public Task<StaffLogin?> GetLoginByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<StaffLogin?>(_logins.TryGetValue(id, out var l) ? l : null);

        public Task UpdateLoginAsync(StaffLogin login, CancellationToken ct = default)
        {
            _logins[login.Id] = login;
            return Task.CompletedTask;
        }

        public Task RevokeAllLoginsForStaffAsync(Guid staffId, DateTimeOffset revokedAt, CancellationToken ct = default)
        {
            foreach (var login in _logins.Values.Where(l => l.StaffId == staffId && l.RevokedAt is null))
                login.RevokedAt = revokedAt;
            return Task.CompletedTask;
        }

        public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        {
            _refreshTokens[token.TokenHash] = token;
            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) =>
            Task.FromResult<RefreshToken?>(_refreshTokens.TryGetValue(tokenHash, out var t) ? t : null);

        public Task UpdateRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        {
            _refreshTokens[token.TokenHash] = token;
            return Task.CompletedTask;
        }
    }
}
