using Application.Commands.AuthCommands;                  // LogoutCommand
using Application.OutBoundPort.AuthRepository.Logins;     // IGetLoginByIdAsync, IUpdateLoginAsync
using Application.UseCases.Auth;                          // LogoutUseCase
using Domain.Entities.AuthEntities;                       // StaffLogin
using Moq;
using MoqTests.TestHelpers;                               // MockTime

namespace MoqTests.UseCases.Auth
{
    public class LogoutMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;

        private readonly Mock<IGetLoginByIdAsync> getLoginById = new();
        private readonly Mock<IUpdateLoginAsync> updateLogin = new();

        private readonly StaffLogin login = new() { StaffId = Guid.NewGuid(), CreatedAt = Now.AddHours(-1), ExpiresAt = Now.AddHours(7) };

        public LogoutMoqTests()
        {
            getLoginById.Setup(g => g.GetLoginByIdAsync(login.Id, It.IsAny<CancellationToken>())).ReturnsAsync(login);
        }

        private LogoutUseCase CreateUseCase() => new(getLoginById.Object, updateLogin.Object, MockTime.At(Now));

        [Fact]
        public async Task Logout_revokes_the_login()
        {
            var result = await CreateUseCase().ExecuteAsync(new LogoutCommand(login.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(Now, login.RevokedAt);
            updateLogin.Verify(u => u.UpdateLoginAsync(login, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Logging_out_twice_is_fine_and_keeps_the_first_logout_time()
        {
            var firstLogout = Now.AddMinutes(-10);
            login.RevokedAt = firstLogout;

            var result = await CreateUseCase().ExecuteAsync(new LogoutCommand(login.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(firstLogout, login.RevokedAt);
            updateLogin.Verify(u => u.UpdateLoginAsync(It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_login_is_fine_and_saves_nothing()
        {
            var result = await CreateUseCase().ExecuteAsync(new LogoutCommand(Guid.NewGuid()), Ct);

            Assert.True(result.IsSuccess);
            updateLogin.Verify(u => u.UpdateLoginAsync(It.IsAny<StaffLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
