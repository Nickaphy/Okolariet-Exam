using Application.Auth;
using Application.Helpers.Interfaces;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Domain.Entities;
using Domain.Entities.AuthEntities;
using Microsoft.Extensions.Options;

namespace Application.Helpers
{
    // Method that actually hands out tokens, either through Login -> new token or through Refresh -> Refreshtoken
    public sealed class TokenIssuer(
    IJwtTokenGenerator jwtTokenGenerator,
    IGenerateRefreshToken generateRefreshToken,
    IHashToken hashToken,
    IAddRefreshTokenAsync addRefreshToken,
    TimeProvider time,
    IOptions<JwtSettings> settings) : ITokenIssuer
    {
        public async Task<AuthTokens> IssueAsync(StaffMember staff, StaffLogin login, CancellationToken ct = default)
        {
            var now = time.GetUtcNow();
            var accessExpires = now.AddMinutes(settings.Value.AccessTokenMinutes);
            var accessToken = jwtTokenGenerator.Generate(staff, login.Id, accessExpires);

            var refreshToken = generateRefreshToken.GenerateRefreshToken();
            await addRefreshToken.AddRefreshTokenAsync(new RefreshToken
            {
                LoginId = login.Id,
                TokenHash = hashToken.HashToken(refreshToken),
                CreatedAt = now,
                ExpiresAt = login.ExpiresAt
            }, ct);

            return new AuthTokens(accessToken, accessExpires, refreshToken, login.ExpiresAt);
        }
    }
}
