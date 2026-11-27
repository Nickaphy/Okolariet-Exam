using Application.Auth;                                       // JwtSettings
using Application.Helpers;                                    // TokenIssuer
using Application.OutBoundPort.AuthRepository.RefreshToken;   // IAddRefreshTokenAsync
using Application.OutBoundPort.AuthRepository.Tokens;         // IJwtTokenGenerator, IGenerateRefreshToken, IHashToken
using Domain.Entities;                                        // StaffMember
using Domain.Entities.AuthEntities;                           // StaffLogin, RefreshToken
using Domain.Enums;                                           // StaffRole
using Microsoft.Extensions.Options;                           // Options
using Moq;
using MoqTests.TestHelpers;                                   // MockTime

namespace MoqTests.UseCases.Auth
{
    // TokenIssuer lives in Application/Helpers, but it is the part of Login and Refresh that hands out the tokens.
    // Spec: access token 15 minutes. The refresh token is stored only as a hash.
    public class TokenIssuerMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;

        private readonly Mock<IJwtTokenGenerator> jwt = new();
        private readonly Mock<IGenerateRefreshToken> generateRefreshToken = new();
        private readonly Mock<IHashToken> hashToken = new();
        private readonly Mock<IAddRefreshTokenAsync> addRefreshToken = new();

        private readonly StaffMember staff = new() { Id = Guid.NewGuid(), Email = "anna@okolariet.dk", Role = StaffRole.Admin };
        private readonly StaffLogin login;

        public TokenIssuerMoqTests()
        {
            login = new StaffLogin { StaffId = staff.Id, CreatedAt = Now, ExpiresAt = Now.AddHours(8) };
            generateRefreshToken.Setup(g => g.GenerateRefreshToken()).Returns("raw-refresh");
            hashToken.Setup(h => h.HashToken("raw-refresh")).Returns("hashed-refresh");
            jwt.Setup(j => j.Generate(It.IsAny<StaffMember>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>())).Returns("jwt");
        }

        private TokenIssuer CreateIssuer() => new(
            jwt.Object, generateRefreshToken.Object, hashToken.Object, addRefreshToken.Object,
            MockTime.At(Now), Options.Create(new JwtSettings()));

        [Fact]
        public async Task Only_the_hash_of_the_refresh_token_is_stored()
        {
            await CreateIssuer().IssueAsync(staff, login, Ct);

            addRefreshToken.Verify(a => a.AddRefreshTokenAsync(
                It.Is<RefreshToken>(t => t.TokenHash == "hashed-refresh"
                                      && t.LoginId == login.Id
                                      && t.CreatedAt == Now
                                      && t.ExpiresAt == login.ExpiresAt
                                      && t.UsedAt == null),
                It.IsAny<CancellationToken>()), Times.Once);
            addRefreshToken.Verify(a => a.AddRefreshTokenAsync(
                It.Is<RefreshToken>(t => t.TokenHash == "raw-refresh"),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Client_gets_the_raw_refresh_token_and_a_15_minute_access_token()
        {
            var tokens = await CreateIssuer().IssueAsync(staff, login, Ct);

            Assert.Equal("jwt", tokens.AccessToken);
            Assert.Equal(Now.AddMinutes(15), tokens.AccessTokenExpiresAt);
            Assert.Equal("raw-refresh", tokens.RefreshToken);
            Assert.Equal(login.ExpiresAt, tokens.RefreshTokenExpiresAt);
            jwt.Verify(j => j.Generate(staff, login.Id, Now.AddMinutes(15)), Times.Once);
        }
    }
}
