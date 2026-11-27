using Application.Auth;                                   // AuthTokens, JwtSettings
using Application.Commands.AuthCommands;                  // LoginCommand
using Application.ErrorMessages;                          // UnauthorizedError, TooManyRequestsError
using Application.Helpers.Interfaces;                     // ITokenIssuer
using Application.OutBoundPort.AuthRepository.Lockouts;   // IGetLoginLockoutAsync, ISaveLoginLockoutAsync
using Application.OutBoundPort.AuthRepository.Logins;     // IAddLoginAsync
using Application.OutBoundPort.IPasswordHash;             // IVerify, IVerifyDummy
using Application.OutBoundPort.IStaffRepository;          // IGetByEmailAsync
using Application.UseCases.Auth;                          // LoginUseCase
using Domain.Entities;                                    // StaffMember
using Domain.Entities.AuthEntities;                       // StaffLogin
using Domain.Enums;                                       // StaffRole
using Domain.ValueObjects;                                // LoginLockout
using Microsoft.Extensions.Options;                       // Options
using Moq;
using MoqTests.TestHelpers;                               // MockTime

namespace MoqTests.UseCases.Auth
{
    // Spec (contracts/openapi.yaml, POST /auth/tokens):
    // - a failed login gives the same 401 whether the e-mail exists or not
    // - after 5 failed attempts for an e-mail: 429 with Retry-After for 15 minutes
    // - the login (refresh token) is valid for 8 hours
    public class LoginMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;
        private const string Email = "anna@okolariet.dk";
        private const string CorrectPassword = "Okolariet#2026";

        private readonly Mock<IGetByEmailAsync> getStaffByEmail = new();
        private readonly Mock<IGetLoginLockoutAsync> getLoginLockout = new();
        private readonly Mock<ISaveLoginLockoutAsync> saveLoginLockout = new();
        private readonly Mock<IAddLoginAsync> addLogin = new();
        private readonly Mock<IVerify> verify = new();
        private readonly Mock<IVerifyDummy> verifyDummy = new();
        private readonly Mock<ITokenIssuer> tokenIssuer = new();

        private readonly StaffMember staff = new() { Id = Guid.NewGuid(), Email = Email, PasswordHash = "stored-hash", Role = StaffRole.Admin };
        private readonly AuthTokens tokens = new("access", Now.AddMinutes(15), "refresh", Now.AddHours(8));

        public LoginMoqTests()
        {
            getStaffByEmail.Setup(g => g.GetByEmailAsync(Email, It.IsAny<CancellationToken>())).ReturnsAsync(staff);
            verify.Setup(v => v.Verify("stored-hash", CorrectPassword)).Returns(true);
            tokenIssuer.Setup(t => t.IssueAsync(It.IsAny<StaffMember>(), It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tokens);
        }

        private LoginUseCase CreateUseCase() => new(
            getStaffByEmail.Object, getLoginLockout.Object, saveLoginLockout.Object, addLogin.Object,
            verify.Object, verifyDummy.Object, tokenIssuer.Object,
            MockTime.At(Now), Options.Create(new JwtSettings()));

        // Puts an existing lockout row for Email "in the database".
        private void ExistingLockout(int failedAttempts, DateTimeOffset? lockedUntil = null)
            => getLoginLockout.Setup(g => g.GetLoginLockoutAsync(Email, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new LoginLockout { Email = Email, FailedAttempts = failedAttempts, LockedUntil = lockedUntil });

        // ---------- Success ----------

        [Fact]
        public async Task Correct_password_returns_tokens_and_creates_an_8_hour_login()
        {
            StaffLogin? created = null;
            addLogin.Setup(a => a.AddLoginAsync(It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()))
                    .Callback<StaffLogin, CancellationToken>((login, _) => created = login)
                    .Returns(Task.CompletedTask);

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(tokens, result.Value);
            Assert.NotNull(created);
            Assert.Equal(staff.Id, created.StaffId);
            Assert.Equal(Now, created.CreatedAt);
            Assert.Equal(Now.AddHours(8), created.ExpiresAt);
            tokenIssuer.Verify(t => t.IssueAsync(staff, created, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Email_is_trimmed_and_lowercased_before_lookup()
        {
            var result = await CreateUseCase().ExecuteAsync(new LoginCommand("  Anna@Okolariet.DK ", CorrectPassword), Ct);

            Assert.True(result.IsSuccess);
            getStaffByEmail.Verify(g => g.GetByEmailAsync(Email, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Successful_login_without_earlier_failures_does_not_write_a_lockout()
        {
            await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(It.IsAny<LoginLockout>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Successful_login_after_failures_resets_the_counter()
        {
            ExistingLockout(failedAttempts: 3);

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            Assert.True(result.IsSuccess);
            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(
                It.Is<LoginLockout>(l => l.FailedAttempts == 0 && l.LockedUntil == null),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ---------- Failed logins ----------

        [Fact]
        public async Task Wrong_password_is_Unauthorized_and_counts_one_failed_attempt()
        {
            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, "Wrong#Password1"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(
                It.Is<LoginLockout>(l => l.Email == Email && l.FailedAttempts == 1 && l.LockedUntil == null),
                It.IsAny<CancellationToken>()), Times.Once);
            addLogin.Verify(a => a.AddLoginAsync(It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);
            tokenIssuer.Verify(t => t.IssueAsync(It.IsAny<StaffMember>(), It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_email_runs_the_dummy_check_and_gives_the_same_error_as_a_wrong_password()
        {
            var unknownEmail = await CreateUseCase().ExecuteAsync(new LoginCommand("ghost@okolariet.dk", "Some#Password1"), Ct);
            var wrongPassword = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, "Some#Password1"), Ct);

            // The dummy check makes an unknown e-mail take as long as a real one, so timing doesn't reveal which e-mails exist.
            verifyDummy.Verify(v => v.VerifyDummy("Some#Password1"), Times.Once);
            Assert.True(unknownEmail.HasError<UnauthorizedError>());
            Assert.Equal(wrongPassword.Errors.Single().Message, unknownEmail.Errors.Single().Message);
        }

        [Fact]
        public async Task Fourth_failed_attempt_does_not_lock()
        {
            ExistingLockout(failedAttempts: 3);

            await CreateUseCase().ExecuteAsync(new LoginCommand(Email, "Wrong#Password1"), Ct);

            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(
                It.Is<LoginLockout>(l => l.FailedAttempts == 4 && l.LockedUntil == null),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Fifth_failed_attempt_locks_the_email_for_15_minutes()
        {
            ExistingLockout(failedAttempts: 4);

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, "Wrong#Password1"), Ct);

            Assert.True(result.HasError<UnauthorizedError>());
            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(
                It.Is<LoginLockout>(l => l.LockedUntil == Now.AddMinutes(15)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ---------- Locked out ----------

        [Fact]
        public async Task Locked_out_email_gets_429_with_RetryAfter_900_and_the_password_is_not_checked()
        {
            ExistingLockout(failedAttempts: 0, lockedUntil: Now.AddMinutes(10));

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            var error = Assert.Single(result.Errors.OfType<TooManyRequestsError>());
            Assert.Equal(900, error.RetryAfterSeconds);
            verify.Verify(v => v.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            verifyDummy.Verify(v => v.VerifyDummy(It.IsAny<string>()), Times.Never);
            tokenIssuer.Verify(t => t.IssueAsync(It.IsAny<StaffMember>(), It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Lockout_ends_exactly_at_LockedUntil()
        {
            ExistingLockout(failedAttempts: 0, lockedUntil: Now);

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Expired_lockout_lets_the_correct_password_in_and_clears_the_lockout()
        {
            ExistingLockout(failedAttempts: 0, lockedUntil: Now.AddMinutes(-1));

            var result = await CreateUseCase().ExecuteAsync(new LoginCommand(Email, CorrectPassword), Ct);

            Assert.True(result.IsSuccess);
            saveLoginLockout.Verify(s => s.SaveLoginLockoutAsync(
                It.Is<LoginLockout>(l => l.FailedAttempts == 0 && l.LockedUntil == null),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
