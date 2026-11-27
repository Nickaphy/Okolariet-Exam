using Application.Auth;                                       // AuthTokens
using Application.Commands.AuthCommands;                      // RefreshTokenCommand
using Application.ErrorMessages;                              // UnauthorizedError
using Application.Helpers.Interfaces;                         // ITokenIssuer
using Application.OutBoundPort.AuthRepository.Logins;         // IGetLoginByIdAsync, IUpdateLoginAsync
using Application.OutBoundPort.AuthRepository.RefreshToken;   // IGetRefreshTokenByHashAsync, IUpdateRefreshTokenAsync
using Application.OutBoundPort.AuthRepository.Tokens;         // IHashToken
using Application.OutBoundPort.IStaffRepository;              // IGetByIdAsync
using Application.UseCases.Auth;                              // RefreshTokensUseCase
using Domain.Entities;                                        // StaffMember
using Domain.Entities.AuthEntities;                           // StaffLogin, RefreshToken
using Domain.Enums;                                           // StaffRole
using FluentResults;                                          // Result
using Moq;
using MoqTests.TestHelpers;                                   // MockTime

namespace MoqTests.UseCases.Auth
{
    // Spec (contracts/openapi.yaml, POST /auth/tokens, grant refresh_token):
    // - the old refresh token is invalidated (rotation)
    // - presenting an already-used refresh token revokes the whole login (theft detection)
    // - the server stores refresh tokens only as a hash
    public class RefreshTokensMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;
        private const string RawToken = "raw-refresh-token";
        private const string HashedToken = "hashed-refresh-token";

        private readonly Mock<IGetRefreshTokenByHashAsync> getRefreshTokenByHash = new();
        private readonly Mock<IUpdateRefreshTokenAsync> updateRefreshToken = new();
        private readonly Mock<IGetLoginByIdAsync> getLoginById = new();
        private readonly Mock<IUpdateLoginAsync> updateLogin = new();
        private readonly Mock<IGetByIdAsync> getStaffById = new();
        private readonly Mock<IHashToken> hashToken = new();
        private readonly Mock<ITokenIssuer> tokenIssuer = new();

        private readonly StaffMember staff = new() { Id = Guid.NewGuid(), Email = "anna@okolariet.dk", Role = StaffRole.Admin };
        private readonly AuthTokens tokens = new("new-access", Now.AddMinutes(15), "new-refresh", Now.AddHours(7));

        public RefreshTokensMoqTests()
        {
            hashToken.Setup(h => h.HashToken(RawToken)).Returns(HashedToken);
            getStaffById.Setup(g => g.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);
            tokenIssuer.Setup(t => t.IssueAsync(It.IsAny<StaffMember>(), It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tokens);
        }

        private RefreshTokensUseCase CreateUseCase() => new(
            getRefreshTokenByHash.Object, updateRefreshToken.Object, getLoginById.Object, updateLogin.Object,
            getStaffById.Object, hashToken.Object, tokenIssuer.Object, MockTime.At(Now));

        private Task<Result<AuthTokens>> Refresh() => CreateUseCase().ExecuteAsync(new RefreshTokenCommand(RawToken), Ct);

        // Puts a login and its refresh token "in the database". Defaults: both valid for 7 more hours, token unused.
        private (StaffLogin Login, RefreshToken Token) ArrangeLogin(
            DateTimeOffset? loginExpiresAt = null, DateTimeOffset? tokenExpiresAt = null, DateTimeOffset? tokenUsedAt = null)
        {
            var login = new StaffLogin { StaffId = staff.Id, CreatedAt = Now.AddHours(-1), ExpiresAt = loginExpiresAt ?? Now.AddHours(7) };
            var token = new RefreshToken
            {
                LoginId = login.Id,
                TokenHash = HashedToken,
                CreatedAt = Now.AddHours(-1),
                ExpiresAt = tokenExpiresAt ?? Now.AddHours(7),
                UsedAt = tokenUsedAt
            };
            getRefreshTokenByHash.Setup(g => g.GetRefreshTokenByHashAsync(HashedToken, It.IsAny<CancellationToken>())).ReturnsAsync(token);
            getLoginById.Setup(g => g.GetLoginByIdAsync(login.Id, It.IsAny<CancellationToken>())).ReturnsAsync(login);
            return (login, token);
        }

        private void AssertNoTokensIssued()
            => tokenIssuer.Verify(t => t.IssueAsync(It.IsAny<StaffMember>(), It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);

        [Fact]
        public async Task Valid_token_is_marked_used_and_a_new_pair_is_issued()
        {
            var (login, token) = ArrangeLogin();

            var result = await Refresh();

            Assert.True(result.IsSuccess);
            Assert.Same(tokens, result.Value);
            Assert.Equal(Now, token.UsedAt);
            updateRefreshToken.Verify(u => u.UpdateRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
            tokenIssuer.Verify(t => t.IssueAsync(staff, login, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Token_is_looked_up_by_its_hash_never_by_the_raw_value()
        {
            ArrangeLogin();

            await Refresh();

            getRefreshTokenByHash.Verify(g => g.GetRefreshTokenByHashAsync(HashedToken, It.IsAny<CancellationToken>()), Times.Once);
            getRefreshTokenByHash.Verify(g => g.GetRefreshTokenByHashAsync(RawToken, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_token_is_Unauthorized()
        {
            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            AssertNoTokensIssued();
        }

        [Fact]
        public async Task Reused_token_revokes_the_whole_login()
        {
            var (login, _) = ArrangeLogin(tokenUsedAt: Now.AddMinutes(-5));

            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            Assert.Equal(Now, login.RevokedAt);
            updateLogin.Verify(u => u.UpdateLoginAsync(login, It.IsAny<CancellationToken>()), Times.Once);
            updateRefreshToken.Verify(u => u.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
            AssertNoTokensIssued();
        }

        [Fact]
        public async Task Expired_token_is_Unauthorized_and_not_rotated()
        {
            var (_, token) = ArrangeLogin(tokenExpiresAt: Now);

            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            Assert.Null(token.UsedAt);
            updateRefreshToken.Verify(u => u.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
            AssertNoTokensIssued();
        }

        [Fact]
        public async Task Logged_out_login_is_Unauthorized()
        {
            var (login, _) = ArrangeLogin();
            login.RevokedAt = Now.AddMinutes(-1);

            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            AssertNoTokensIssued();
        }

        [Fact]
        public async Task Login_older_than_its_8_hours_is_Unauthorized()
        {
            ArrangeLogin(loginExpiresAt: Now);

            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            AssertNoTokensIssued();
        }

        [Fact]
        public async Task Deleted_staff_member_is_Unauthorized()
        {
            ArrangeLogin();
            getStaffById.Setup(g => g.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync((StaffMember?)null);

            var result = await Refresh();

            Assert.True(result.HasError<UnauthorizedError>());
            AssertNoTokensIssued();
        }
    }
}
