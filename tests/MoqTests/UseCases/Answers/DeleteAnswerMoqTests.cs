using Application.Commands.AnswerCommands;             // DeleteAnswerCommand
using Application.ErrorMessages;                       // NotFoundError
using Application.OutBoundPort.IQuestionRepository;    // IGetQuestionByAnswerIdAsync, IUpdateQuestionAsync
using Application.UseCases.Answers;                    // DeleteAnswerUseCase
using Domain.Entities;                                 // Question, Answer
using Moq;

namespace MoqTests.UseCases.Answers
{
    // Spec (contracts/openapi.yaml, DELETE /answers/{id}): soft delete (IsActive = false), so statistics survive.
    public class DeleteAnswerMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetQuestionByAnswerIdAsync> getQuestionByAnswer = new();
        private readonly Mock<IUpdateQuestionAsync> update = new();

        private readonly Question question = Question.Create(Guid.NewGuid(), "Hvad spiser en sæl?", null, 1);
        private readonly Answer answer;

        public DeleteAnswerMoqTests()
        {
            answer = question.AddAnswer("Fisk", true, null, 1);
            getQuestionByAnswer.Setup(g => g.GetQuestionByAnswerIdAsync(answer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        }

        private DeleteAnswerUseCase CreateUseCase() => new(getQuestionByAnswer.Object, update.Object);

        [Fact]
        public async Task Delete_is_a_soft_delete_that_keeps_the_answer_on_the_question()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteAnswerCommand(answer.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.False(answer.IsActive);
            Assert.Same(answer, Assert.Single(question.Answers));
            update.Verify(u => u.UpdateQuestionAsync(question, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleting_an_already_deleted_answer_is_NotFound()
        {
            answer.Deactivate();

            var result = await CreateUseCase().DeleteAsync(new DeleteAnswerCommand(answer.Id), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_answer_is_NotFound()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteAnswerCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
