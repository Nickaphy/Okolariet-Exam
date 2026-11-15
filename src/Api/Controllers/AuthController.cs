using Api.Helpers;
using Api.Requests.AuthRequests;
using Api.Responses.AuthResponses;
using Application.Auth;
using Application.Commands.AuthCommands;
using Application.InBoundPort.AuthUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    [ApiController]
    [Route("auth/tokens")]
    public sealed class AuthController : ControllerBase
    {
        [HttpPost, AllowAnonymous, EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request,
            [FromServices] ILoginUseCase useCase, CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(new LoginCommand(request.Email, request.Password), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }

        [HttpPost("refresh"), AllowAnonymous, EnableRateLimiting("auth")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokensRequest request,
            [FromServices] IRefreshTokensUseCase useCase, CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(new RefreshTokenCommand(request.RefreshToken), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }

        [HttpDelete("current"), Authorize]   // any logged-in user
        public async Task<IActionResult> Logout([FromServices] ILogoutUseCase useCase, CancellationToken ct)
        {
            await useCase.ExecuteAsync(new LogoutCommand(User.GetLoginId()), ct);
            return NoContent();
        }

        private IActionResult Tokens(AuthTokens t)
        {
            Response.Headers.CacheControl = "no-store";
            var expiresIn = (int)(t.AccessTokenExpiresAt - DateTimeOffset.UtcNow).TotalSeconds;
            return Ok(new TokenResponse(t.AccessToken, "Bearer", expiresIn, t.RefreshToken));
        }
    }
}
