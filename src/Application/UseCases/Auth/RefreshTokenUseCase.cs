using Application.Auth;
using Application.Commands.AuthCommands;
using Application.ErrorMessages;
using Application.Helpers.Interfaces;
using Application.InBoundPort.AuthUseCases;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Application.OutBoundPort.IStaffRepository;
using FluentResults;

namespace Application.UseCases.Auth
{
    public sealed class RefreshTokensUseCase : IRefreshTokensUseCase // Refresh tokens are used so that a user doesn't have to log in every 15 minutes, instead it detects activity and refresh' the token
    {
        private readonly IGetRefreshTokenByHashAsync getRefreshTokenByHash;
        private readonly IUpdateRefreshTokenAsync updateRefreshToken;
        private readonly IGetLoginByIdAsync getLoginById;
        private readonly IUpdateLoginAsync updateLogin;
        private readonly IGetByIdAsync getStaffById;
        private readonly IHashToken hashToken;
        private readonly ITokenIssuer tokenIssuer;
        private readonly TimeProvider time;
        public RefreshTokensUseCase(
                IGetRefreshTokenByHashAsync getRefreshTokenByHash,
                IUpdateRefreshTokenAsync updateRefreshToken,
                IGetLoginByIdAsync getLoginById,
                IUpdateLoginAsync updateLogin,
                IGetByIdAsync getStaffById,
                IHashToken hashToken,
                ITokenIssuer tokenIssuer,
                TimeProvider time)
        {
            this.getRefreshTokenByHash = getRefreshTokenByHash;
            this.updateRefreshToken = updateRefreshToken;
            this.getLoginById = getLoginById;
            this.updateLogin = updateLogin;
            this.getStaffById = getStaffById;
            this.hashToken = hashToken;
            this.tokenIssuer = tokenIssuer;
            this.time = time;
        }
        public async Task<Result<AuthTokens>> ExecuteAsync(RefreshTokenCommand command, CancellationToken ct = default)
        {
            var now = time.GetUtcNow();
            var unauthorized = Result.Fail(new UnauthorizedError("Ugyldig session."));

            var stored = await getRefreshTokenByHash.GetRefreshTokenByHashAsync(hashToken.HashToken(command.RefreshToken), ct); // Find the hashed token
            if (stored is null) return unauthorized;

            var login = await getLoginById.GetLoginByIdAsync(stored.LoginId, ct);
            if (login is null || !login.IsActive(now)) return unauthorized;

            if (stored.UsedAt is not null)            // reuse detected: revoke the whole login
            {
                login.RevokedAt = now;
                await updateLogin.UpdateLoginAsync(login, ct);
                return unauthorized;
            }
            if (stored.ExpiresAt <= now) return unauthorized;

            stored.UsedAt = now;
            await updateRefreshToken.UpdateRefreshTokenAsync(stored, ct);

            var staff = await getStaffById.GetByIdAsync(login.StaffId, ct);
            if (staff is null) return unauthorized;

            return await tokenIssuer.IssueAsync(staff, login, ct);
        }
    }
}
