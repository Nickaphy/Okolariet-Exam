using Application.Commands.QuizResultCommands;
using Application.ErrorMessages;
using Application.UseCases.QuizResults;
using Domain.Entities;
using xUnitTests.ApplicationTests.Fakes;

namespace xUnitTests.ApplicationTests.UseCases
{
    public class QuizResultUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private readonly FakeQuizResultRepository repo = new();
        private readonly SubmitQuizResultUseCase useCase;
        private readonly SubmitQuizResultCommand command;
        private readonly Answer wrongAnswer;

        public QuizResultUseCaseTests()
        {
            var sessions = new FakeSessionRepository();
            var session = Session.Create(sessions.ProjectId);
            sessions.Session = session;
            repo.ProjectId = session.ProjectId;
            var questions = new FakeQuestionRepository();
            var question = TestData.Question(repo.QuizId);
            var correctAnswer = question.AddAnswer("8", true, null, 0);
            wrongAnswer = question.AddAnswer("6", false, null, 1);
            questions.Questions.Add(question);
            useCase = new SubmitQuizResultUseCase(sessions, questions, repo, repo);
            command = new SubmitQuizResultCommand(session.Id, question.Id, correctAnswer.Id);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Submit_records_whether_answer_was_correct(bool isCorrect)
        {
            var submission = isCorrect ? command : command with { AnswerId = wrongAnswer.Id };

            var result = await useCase.SubmitAsync(submission, Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(isCorrect, Assert.Single(repo.Results).IsCorrect);
            Assert.Equal(command.AnswerId, result.Value.CorrectAnswerId);
        }

        [Fact]
        public async Task Submit_unknown_answer_fails_and_saves_nothing()
        {
            var result = await useCase.SubmitAsync(command with { AnswerId = Guid.NewGuid() }, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(repo.Results);
        }

        [Fact]
        public async Task Submit_twice_is_Conflict()
        {
            var first = await useCase.SubmitAsync(command, Ct);
            var second = await useCase.SubmitAsync(command, Ct);

            Assert.True(first.IsSuccess);
            Assert.True(second.HasError<ConflictError>());
            Assert.Single(repo.Results);
        }
    }
}
