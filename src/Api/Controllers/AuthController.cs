using Api.Helpers;
using Api.Requests.AuthRequests;
using Api.Responses.AuthResponses;
using Application.Auth;
using Application.Commands.AuthCommands;
using Application.InBoundPort.AuthUseCases;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    // Log in, renew a login, log out.
    // TO-DO create Password Reset
    [ApiController]
    [Route("api/v1/auth/tokens")]
    public sealed class AuthController : ControllerBase
    {
        // POST /auth/tokens. The grant type decides what happens (see TokenRequest).
        // Anonymous (you aren't logged in yet) and rate limited per client.
        [HttpPost, AllowAnonymous, EnableRateLimiting("auth")]
        public async Task<IActionResult> CreateToken([FromBody] TokenRequest request,
            [FromServices] ILoginUseCase login,
            [FromServices] IRefreshTokensUseCase refresh,
            CancellationToken ct)
        {
            Result<AuthTokens> result;
            switch (request.GrantType)
            {
                case TokenRequest.PasswordGrant:
                    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
                        return Invalid("email and password are required for the password grant.");
                    result = await login.ExecuteAsync(new LoginCommand(request.Email, request.Password), ct);
                    break;

                case TokenRequest.RefreshTokenGrant:
                    if (string.IsNullOrWhiteSpace(request.RefreshToken))
                        return Invalid("refreshToken is required for the refresh_token grant.");
                    result = await refresh.ExecuteAsync(new RefreshTokenCommand(request.RefreshToken), ct);
                    break;

                default:
                    return Invalid("grantType must be 'password' or 'refresh_token'.");
            }

            if (result.IsFailed) return this.ToProblem(result);

            // 201 Created, as in the contract. Tokens must never be cached by a browser or proxy.
            var t = result.Value;
            Response.Headers.CacheControl = "no-store";
            return StatusCode(StatusCodes.Status201Created,
                new TokenResponse(t.AccessToken, "Bearer", t.AccessTokenExpiresAt, t.RefreshToken, t.RefreshTokenExpiresAt));
        }

        // DELETE /auth/tokens/current: log out. [Authorize] = any logged-in staff member.
        // The login is revoked, so its refresh token stops working. The access token itself is stateless
        // and works until it expires (at most 15 minutes), so the editor throws it away too.
        [HttpDelete("current"), Authorize]
        public async Task<IActionResult> Logout([FromServices] ILogoutUseCase useCase, CancellationToken ct)
        {
            await useCase.ExecuteAsync(new LogoutCommand(User.GetLoginId()), ct);
            return NoContent();
        }

        // 400 with a ProblemDetails, the same shape as the automatic model validation errors.
        private IActionResult Invalid(string detail) =>
            Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
    }
}
