using System.Timers;
using Application.Auth;
using Application.Commands.AuthCommands;
using Application.ErrorMessages;
using Application.Helpers;
using Application.Helpers.Interfaces;
using Application.InBoundPort.AuthUseCases;
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities.AuthEntities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Auth
{
    public sealed class LoginUseCase : ILoginUseCase
    {
        private readonly IGetByEmailAsync getStaffByEmail;
        private readonly IGetLoginLockoutAsync getLoginLockout;
        private readonly ISaveLoginLockoutAsync saveLoginLockout;
        private readonly IAddLoginAsync addLogin;
        private readonly IVerify verify;
        private readonly IVerifyDummy verifyDummy;
        private readonly ITokenIssuer tokenIssuer;
        private readonly TimeProvider time;
        private readonly IOptions<JwtSettings> settings;
        public LoginUseCase(IGetByEmailAsync getStaffByEmail,
                            IGetLoginLockoutAsync getLoginLockout,
                            ISaveLoginLockoutAsync saveLoginLockout,
                            IAddLoginAsync addLogin,
                            IVerify verify,
                            IVerifyDummy verifyDummy,
                            ITokenIssuer tokenIssuer,
                            TimeProvider time,
                            IOptions<JwtSettings> settings)
        {
            this.getStaffByEmail = getStaffByEmail;
            this.getLoginLockout = getLoginLockout;
            this.saveLoginLockout = saveLoginLockout;
            this.addLogin = addLogin;
            this.verify = verify;
            this.verifyDummy = verifyDummy;
            this.tokenIssuer = tokenIssuer;
            this.time = time;
            this.settings = settings;
        }
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public async Task<Result<AuthTokens>> ExecuteAsync(LoginCommand command, CancellationToken ct = default)
        {
            var email = EmailNormalizer.Normalize(command.Email); //Normalize email to prevent wierd uppercase
            var now = time.GetUtcNow();

            var lockout = await getLoginLockout.GetLoginLockoutAsync(email, ct) ?? new LoginLockout { Email = email }; // Validates whether user has tried to many times, and is thus locked out.
            if (lockout.LockedUntil > now)
                return Result.Fail(new TooManyRequestsError("For mange log-in forsøg, prøv igen om 15 minutter", 15*60));

            var staff = await getStaffByEmail.GetByEmailAsync(email, ct); //Get staff by email, emails are always unique
            var valid = staff is null
                ? verifyDummy.VerifyDummy(command.Password) //Verify validity
                : verify.Verify(staff.PasswordHash, command.Password);

            if (!valid) // Counts the amount of failed attempts, if it's 5, they get locked out
            {
                lockout.FailedAttempts++;
                if (lockout.FailedAttempts >= MaxFailedAttempts)
                {
                    lockout.LockedUntil = now + LockoutDuration;
                    lockout.FailedAttempts = 0;
                }
                await saveLoginLockout.SaveLoginLockoutAsync(lockout, ct);
                return Result.Fail(new UnauthorizedError("Forkert e-mail eller adgangskode."));
            }

            if (lockout.FailedAttempts > 0 || lockout.LockedUntil is not null) // saves the amount
            {
                lockout.FailedAttempts = 0;
                lockout.LockedUntil = null;
                await saveLoginLockout.SaveLoginLockoutAsync(lockout, ct);
            }

            var login = new StaffLogin //Actually creates the login token
            {
                StaffId = staff!.Id,
                CreatedAt = now,
                ExpiresAt = now.AddHours(settings.Value.RefreshTokenHours)
            };
            await addLogin.AddLoginAsync(login, ct);

            return await tokenIssuer.IssueAsync(staff, login, ct);
        }
    }
}
