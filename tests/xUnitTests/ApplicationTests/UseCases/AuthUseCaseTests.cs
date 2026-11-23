using Application.Auth;                        // JwtSettings
using Application.Commands.AuthCommands;       // LoginCommand, RefreshTokenCommand, LogoutCommand
using Application.ErrorMessages;               // UnauthorizedError, TooManyRequestsError
using Application.Helpers;                     // TokenIssuer
using Application.UseCases.Auth;               // LoginUseCase, RefreshTokensUseCase, LogoutUseCase
using Domain.Entities;                         // StaffMember
using Domain.Entities.AuthEntities;            // StaffLogin, RefreshToken
using Domain.Enums;                            // StaffRole
using Domain.ValueObjects;                     // LoginLockout
using Microsoft.Extensions.Options;            // Options.Create
using xUnitTests.ApplicationTests.Fakes;       // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class AuthUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = new(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);

        private readonly FakeStaffRepository staff = new();
        private readonly FakePasswordHasher hasher = new();
        private readonly FakeAuthStore store = new();
        private readonly FakeTokenIssuer issuer = new();
        private readonly FakeTokenTools tokenTools = new();
        private readonly IOptions<JwtSettings> settings = Options.Create(new JwtSettings { AccessTokenMinutes = 15, RefreshTokenHours = 8 });

        // A staff member whose password is "secret".
        private StaffMember AddAnna()
        {
            var anna = new StaffMember { Id = Guid.NewGuid(), Email = "anna@test.dk", PasswordHash = "hashed-secret", Role = StaffRole.Admin };
            staff.Staff.Add(anna);
            return anna;
        }

        private LoginUseCase Login() =>
            new(staff, store, store, store, hasher, hasher, issuer, new FixedTimeProvider(Now), settings);

        private RefreshTokensUseCase Refresh() =>
            new(store, store, store, store, staff, tokenTools, issuer, new FixedTimeProvider(Now));

        // An active login for anna with one unused refresh token whose raw value is "token".
        private (StaffLogin login, RefreshToken token) AddSession(StaffMember owner)
        {
            var login = new StaffLogin { StaffId = owner.Id, CreatedAt = Now.AddHours(-1), ExpiresAt = Now.AddHours(7) };
            var token = new RefreshToken { LoginId = login.Id, TokenHash = "hash(token)", CreatedAt = Now.AddHours(-1), ExpiresAt = login.ExpiresAt };
            store.Logins.Add(login);
            store.RefreshTokens.Add(token);
            return (login, token);
        }

        // ---------- Login ----------

        [Fact]
        public async Task Login_with_correct_password_creates_a_login_and_issues_tokens()
        {
            var anna = AddAnna();

            var result = await Login().ExecuteAsync(new LoginCommand("anna@test.dk", "secret"), Ct);

            Assert.True(result.IsSuccess);
            var login = Assert.Single(store.Logins);
            Assert.Equal(anna.Id, login.StaffId);
            Assert.Equal(Now, login.CreatedAt);
            Assert.Equal(Now.AddHours(8), login.ExpiresAt);    // RefreshTokenHours
            Assert.Same(anna, issuer.IssuedFor);
            Assert.Same(login, issuer.IssuedForLogin);
        }

        [Fact]
        public async Task Login_normalises_the_email()
        {
            AddAnna();

            var result = await Login().ExecuteAsync(new LoginCommand("  ANNA@Test.DK ", "secret"), Ct);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Login_with_wrong_password_is_Unauthorized_and_counts_the_attempt()
        {
            AddAnna();

            var result = await Login().ExecuteAsync(new LoginCommand("anna@test.dk", "wrong"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
            Assert.Equal(1, Assert.Single(store.Lockouts).FailedAttempts);
            Assert.Empty(store.Logins);
        }

        [Fact]
        public async Task Login_with_unknown_email_gives_the_same_error_and_still_runs_a_dummy_check()
        {
            AddAnna();

            var unknown = await Login().ExecuteAsync(new LoginCommand("nobody@test.dk", "secret"), Ct);
            var wrongPassword = await Login().ExecuteAsync(new LoginCommand("anna@test.dk", "wrong"), Ct);

            // Same message, so the response doesn't reveal which e-mails exist.
            Assert.True(unknown.HasError<UnauthorizedError>());
            Assert.Equal(wrongPassword.Errors[0].Message, unknown.Errors[0].Message);
            // The dummy hash check keeps the response time the same as for a real user.
            Assert.True(hasher.DummyWasCalled);
        }

        [Fact]
        public async Task Fifth_failed_attempt_locks_the_email_for_15_minutes()
        {
            AddAnna();
            var useCase = Login();

            for (var i = 0; i < 5; i++)
                await useCase.ExecuteAsync(new LoginCommand("anna@test.dk", "wrong"), Ct);

            var lockout = Assert.Single(store.Lockouts);
            Assert.Equal(Now.AddMinutes(15), lockout.LockedUntil);
            Assert.Equal(0, lockout.FailedAttempts);   // counter restarts after locking
        }

        [Fact]
        public async Task Locked_email_is_TooManyRequests_even_with_the_correct_password()
        {
            AddAnna();
            store.Lockouts.Add(new LoginLockout { Email = "anna@test.dk", LockedUntil = Now.AddMinutes(10) });

            var result = await Login().ExecuteAsync(new LoginCommand("anna@test.dk", "secret"), Ct);

            var error = Assert.Single(result.Errors.OfType<TooManyRequestsError>());
            Assert.Equal(15 * 60, error.RetryAfterSeconds);
            Assert.Empty(store.Logins);
        }

        [Fact]
        public async Task Expired_lock_lets_the_user_in_and_clears_the_lock()
        {
            AddAnna();
            var lockout = new LoginLockout { Email = "anna@test.dk", FailedAttempts = 2, LockedUntil = Now.AddMinutes(-1) };
            store.Lockouts.Add(lockout);

            var result = await Login().ExecuteAsync(new LoginCommand("anna@test.dk", "secret"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Null(lockout.LockedUntil);
            Assert.Equal(0, lockout.FailedAttempts);
        }

        // ---------- Refresh ----------

        [Fact]
        public async Task Refresh_with_valid_token_marks_it_used_and_issues_new_tokens()
        {
            var anna = AddAnna();
            var (login, token) = AddSession(anna);

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("token"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(Now, token.UsedAt);
            Assert.Same(login, issuer.IssuedForLogin);   // same login (sid), new token pair
        }

        [Fact]
        public async Task Refresh_with_unknown_token_is_Unauthorized()
        {
            AddSession(AddAnna());

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("made-up"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
            Assert.Null(issuer.IssuedFor);
        }

        [Fact]
        public async Task Reusing_a_refresh_token_revokes_the_whole_login()
        {
            var (login, token) = AddSession(AddAnna());
            token.UsedAt = Now.AddMinutes(-5);   // already used once: someone replayed it

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("token"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
            Assert.Equal(Now, login.RevokedAt);
            Assert.Equal(1, store.LoginUpdates);
        }

        [Fact]
        public async Task Refresh_on_a_revoked_login_is_Unauthorized()
        {
            var (login, _) = AddSession(AddAnna());
            login.RevokedAt = Now.AddMinutes(-1);

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("token"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
        }

        [Fact]
        public async Task Refresh_after_the_8_hour_login_has_expired_is_Unauthorized()
        {
            var anna = AddAnna();
            var login = new StaffLogin { StaffId = anna.Id, CreatedAt = Now.AddHours(-9), ExpiresAt = Now.AddHours(-1) };
            store.Logins.Add(login);
            store.RefreshTokens.Add(new RefreshToken { LoginId = login.Id, TokenHash = "hash(token)", ExpiresAt = login.ExpiresAt });

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("token"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
        }

        [Fact]
        public async Task Refresh_for_a_deleted_staff_member_is_Unauthorized()
        {
            var anna = AddAnna();
            AddSession(anna);
            staff.Staff.Remove(anna);

            var result = await Refresh().ExecuteAsync(new RefreshTokenCommand("token"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
        }

        // ---------- Logout ----------

        [Fact]
        public async Task Logout_revokes_the_login()
        {
            var (login, _) = AddSession(AddAnna());
            var useCase = new LogoutUseCase(store, store, new FixedTimeProvider(Now));

            var result = await useCase.ExecuteAsync(new LogoutCommand(login.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(Now, login.RevokedAt);
        }

        [Fact]
        public async Task Logout_twice_is_fine_and_keeps_the_first_revoke_time()
        {
            var (login, _) = AddSession(AddAnna());
            var earlier = Now.AddMinutes(-30);
            login.RevokedAt = earlier;
            var useCase = new LogoutUseCase(store, store, new FixedTimeProvider(Now));

            var result = await useCase.ExecuteAsync(new LogoutCommand(login.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(earlier, login.RevokedAt);
            Assert.Equal(0, store.LoginUpdates);
        }

        [Fact]
        public async Task Logout_with_unknown_login_is_still_Ok()
        {
            var useCase = new LogoutUseCase(store, store, new FixedTimeProvider(Now));

            var result = await useCase.ExecuteAsync(new LogoutCommand(Guid.NewGuid()), Ct);

            Assert.True(result.IsSuccess);
        }

        // ---------- TokenIssuer ----------

        [Fact]
        public async Task TokenIssuer_returns_a_15_minute_access_token_and_the_raw_refresh_token()
        {
            var anna = AddAnna();
            var login = new StaffLogin { StaffId = anna.Id, CreatedAt = Now, ExpiresAt = Now.AddHours(8) };
            var tokenIssuer = new TokenIssuer(tokenTools, tokenTools, tokenTools, store, new FixedTimeProvider(Now), settings);

            var tokens = await tokenIssuer.IssueAsync(anna, login, Ct);

            Assert.Equal($"jwt-{anna.Id}-{login.Id}", tokens.AccessToken);
            Assert.Equal(Now.AddMinutes(15), tokens.AccessTokenExpiresAt);
            Assert.Equal("raw-refresh-token", tokens.RefreshToken);
            Assert.Equal(login.ExpiresAt, tokens.RefreshTokenExpiresAt);
        }

        [Fact]
        public async Task TokenIssuer_stores_only_the_hash_of_the_refresh_token()
        {
            var anna = AddAnna();
            var login = new StaffLogin { StaffId = anna.Id, CreatedAt = Now, ExpiresAt = Now.AddHours(8) };
            var tokenIssuer = new TokenIssuer(tokenTools, tokenTools, tokenTools, store, new FixedTimeProvider(Now), settings);

            await tokenIssuer.IssueAsync(anna, login, Ct);

            var stored = Assert.Single(store.RefreshTokens);
            Assert.Equal("hash(raw-refresh-token)", stored.TokenHash);
            Assert.Equal(login.Id, stored.LoginId);
            Assert.Equal(login.ExpiresAt, stored.ExpiresAt);   // can't outlive the login
            Assert.Null(stored.UsedAt);
        }
    }
}
