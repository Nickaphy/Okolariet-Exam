using Application.Commands.Sessions;
using Application.ErrorMessages;
using Application.Models;
using Application.UseCases.Sessions;
using xUnitTests.ApplicationTests.Fakes;

namespace xUnitTests.ApplicationTests.UseCases
{
    public class SessionUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private readonly FakeSessionRepository repo = new();

        [Fact]
        public async Task Add_saves_session_for_existing_project()
        {
            var useCase = new AddSessionUseCase(repo, repo);

            var result = await useCase.PostSessionAsync(new CreateSessionCommand(repo.ProjectId), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(repo.Session, result.Value);
            Assert.Equal(repo.ProjectId, result.Value.ProjectId);
        }

        [Fact]
        public async Task Add_with_unknown_project_is_NotFound()
        {
            var useCase = new AddSessionUseCase(repo, repo);

            var result = await useCase.PostSessionAsync(new CreateSessionCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            Assert.Null(repo.Session);
        }

        [Fact]
        public async Task Get_returns_session_summary()
        {
            var summary = new SessionSummary(Guid.NewGuid(), DateTime.UnixEpoch, 4, 2, 1, 25, 50);
            repo.Summary = summary;
            var useCase = new GetSessionUseCase(repo);

            var result = await useCase.GetAsync(summary.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(summary, result.Value);
        }
    }
}
