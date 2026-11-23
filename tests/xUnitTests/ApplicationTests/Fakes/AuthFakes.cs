using Application.Auth;
using Application.Helpers.Interfaces;
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Domain.Entities.AuthEntities;
using Domain.ValueObjects;

namespace xUnitTests.ApplicationTests.Fakes
{
    // Staff table as a list.
    public class FakeStaffRepository : IGetByEmailAsync, IGetByIdAsync
    {
        public List<StaffMember> Staff { get; } = new();

        public Task<StaffMember?> GetByEmailAsync(string normalizedEmail, CancellationToken ct)
            => Task.FromResult(Staff.FirstOrDefault(s => s.Email == normalizedEmail));

        public Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken ct)
            => Task.FromResult(Staff.FirstOrDefault(s => s.Id == id));
    }

    // "Hashing" is just a prefix, so a test can write PasswordHash = "hashed-secret" and log in with "secret".
    public class FakePasswordHasher : IVerify, IVerifyDummy
    {
        public bool DummyWasCalled { get; private set; }

        public bool Verify(string passwordHash, string password) => passwordHash == "hashed-" + password;

        public bool VerifyDummy(string password) { DummyWasCalled = true; return false; }
    }

    // Logins, refresh tokens and lockouts as lists.
    public class FakeAuthStore :
        IGetLoginLockoutAsync, ISaveLoginLockoutAsync,
        IAddLoginAsync, IGetLoginByIdAsync, IUpdateLoginAsync,
        IAddRefreshTokenAsync, IGetRefreshTokenByHashAsync, IUpdateRefreshTokenAsync
    {
        public List<LoginLockout> Lockouts { get; } = new();
        public List<StaffLogin> Logins { get; } = new();
        public List<RefreshToken> RefreshTokens { get; } = new();
        public int LoginUpdates { get; private set; }

        public Task<LoginLockout?> GetLoginLockoutAsync(string normalizedEmail, CancellationToken ct = default)
            => Task.FromResult(Lockouts.FirstOrDefault(l => l.Email == normalizedEmail));

        public Task SaveLoginLockoutAsync(LoginLockout lockout, CancellationToken ct = default)
        {
            if (!Lockouts.Contains(lockout)) Lockouts.Add(lockout);
            return Task.CompletedTask;
        }

        public Task AddLoginAsync(StaffLogin login, CancellationToken ct = default) { Logins.Add(login); return Task.CompletedTask; }

        public Task<StaffLogin?> GetLoginByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Logins.FirstOrDefault(l => l.Id == id));

        public Task UpdateLoginAsync(StaffLogin login, CancellationToken ct = default) { LoginUpdates++; return Task.CompletedTask; }

        public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default) { RefreshTokens.Add(token); return Task.CompletedTask; }

        public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default)
            => Task.FromResult(RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash));

        public Task UpdateRefreshTokenAsync(RefreshToken token, CancellationToken ct = default) => Task.CompletedTask;
    }

    // Predictable token "crypto": the hash of "abc" is "hash(abc)".
    public class FakeTokenTools : IJwtTokenGenerator, IGenerateRefreshToken, IHashToken
    {
        public string Generate(StaffMember staff, Guid loginId, DateTimeOffset expiresAt) => $"jwt-{staff.Id}-{loginId}";

        public string GenerateRefreshToken() => "raw-refresh-token";

        public string HashToken(string token) => $"hash({token})";
    }

    // Records who it was asked to issue tokens for, and returns fixed tokens.
    public class FakeTokenIssuer : ITokenIssuer
    {
        public StaffMember? IssuedFor { get; private set; }
        public StaffLogin? IssuedForLogin { get; private set; }

        public Task<AuthTokens> IssueAsync(StaffMember staff, StaffLogin login, CancellationToken ct = default)
        {
            IssuedFor = staff;
            IssuedForLogin = login;
            return Task.FromResult(new AuthTokens("access", login.ExpiresAt, "refresh", login.ExpiresAt));
        }
    }
}
