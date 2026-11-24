using Application.Commands.UserCommands;
using Application.ErrorMessages;
using Application.UseCases.Users;
using Domain.Entities;
using Domain.ValueObjects;
using xUnitTests.ApplicationTests.Fakes;

namespace xUnitTests.ApplicationTests.UseCases
{
    public class UserUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private readonly FakeUserRepository repo = new() { QuizId = Guid.NewGuid() };

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Create_saves_score_only_when_quiz_is_complete(bool complete)
        {
            var sessions = new FakeSessionRepository();
            var session = Session.Create(sessions.ProjectId);
            sessions.Session = session;
            var results = new FakeQuizResultRepository { ProjectId = session.ProjectId };
            var questions = new FakeQuestionRepository();
            var question = TestData.Question(results.QuizId);
            var answer = question.AddAnswer("8", true, null, 0);
            questions.Questions.Add(question);
            var firstWord = NicknameWord.Create("blå");
            var secondWord = NicknameWord.Create("kat");
            repo.Words.AddRange([firstWord, secondWord]);
            if (complete)
                results.Results.Add(QuizResult.Create(session.Id, question, answer));
            var useCase = new CreateUserUseCase(repo, repo, sessions, results, questions, results);
            var command = new CreateUserCommand(session.Id, results.QuizId, firstWord.Id, secondWord.Id, 7);

            var result = await useCase.PostUserAsync(command, Ct);

            if (complete)
            {
                Assert.True(result.IsSuccess);
                Assert.Same(repo.User, result.Value);
                Assert.Equal("BlåKat7", result.Value.NickName);
                Assert.Equal(100, result.Value.Score.Percentage);
            }
            else
            {
                Assert.True(result.HasError<BusinessRuleError>());
                Assert.Null(repo.User);
            }
        }

        [Fact]
        public async Task Get_returns_saved_user()
        {
            var user = User.Create("BlåKat7", repo.QuizId, Guid.NewGuid(), new Score(1, 2));
            repo.User = user;

            var result = await new GetUserUseCase(repo).GetAsync(user.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(user, result.Value);
        }

        [Fact]
        public async Task List_passes_filter_sort_and_paging_to_repository()
        {
            var result = await new ListUsersUseCase(repo).ListAsync(repo.QuizId, "bestScore", 2, 10, Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(((Guid?)repo.QuizId, "bestScore", 2, 10), repo.ListRequest!.Value);
        }

        [Fact]
        public async Task List_with_invalid_page_fails()
        {
            var result = await new ListUsersUseCase(repo).ListAsync(null, "newest", 0, 10, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Null(repo.ListRequest);
        }

        [Fact]
        public async Task Delete_removes_saved_user()
        {
            var user = User.Create("BlåKat7", repo.QuizId, Guid.NewGuid(), new Score(1, 2));
            repo.User = user;

            var result = await new DeleteUserUseCase(repo).DeleteAsync(user.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Null(repo.User);
        }

        [Fact]
        public async Task Clear_removes_quiz_leaderboard_entry()
        {
            repo.User = User.Create("BlåKat7", repo.QuizId, Guid.NewGuid(), new Score(1, 2));

            var result = await new ClearQuizLeaderboardUseCase(repo).ClearAsync(repo.QuizId, Ct);

            Assert.True(result.IsSuccess);
            Assert.Null(repo.User);
        }
    }
}
