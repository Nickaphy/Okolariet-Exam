using Application.Commands.Sessions;                   // CreateSessionCommand
using Application.ErrorMessages;                       // NotFoundError
using Application.OutBoundPort.ISessionRepository;     // IProjectExists, ICreateSessionAsync
using Application.UseCases.Sessions;                   // AddSessionUseCase
using Domain.Entities;                                 // Session
using Moq;

// The folder is "Sessions" (not "Session" like in src), because a namespace called
// MoqTests.UseCases.Session would hide the Session class inside this file.
namespace MoqTests.UseCases.Sessions
{
    public class AddSessionMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IProjectExists> projectExists = new();
        private readonly Mock<ICreateSessionAsync> createSession = new();

        private AddSessionUseCase CreateUseCase() => new(projectExists.Object, createSession.Object);

        [Fact]
        public async Task Session_for_an_existing_project_is_created_and_saved()
        {
            var projectId = Guid.NewGuid();
            projectExists.Setup(p => p.ProjectExistsAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().PostSessionAsync(new CreateSessionCommand(projectId), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(projectId, result.Value.ProjectId);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
            createSession.Verify(c => c.CreateSessionAsync(result.Value, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Unknown_project_is_NotFound_and_no_session_is_saved()
        {
            var result = await CreateUseCase().PostSessionAsync(new CreateSessionCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            createSession.Verify(c => c.CreateSessionAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
