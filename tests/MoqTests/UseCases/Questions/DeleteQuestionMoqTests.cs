using Application.Commands.QuestionCommands;           // DeleteQuestionCommand
using Application.ErrorMessages;                       // NotFoundError
using Application.OutBoundPort.IQuestionRepository;    // IGetQuestionByIdAsync, IUpdateQuestionAsync
using Application.UseCases.Questions;                  // DeleteQuestionUseCase
using Domain.Entities;                                 // Question
using Moq;

namespace MoqTests.UseCases.Questions
{
    // Spec (contracts/openapi.yaml, DELETE /questions/{id}): soft delete (IsActive = false), so statistics survive.
    public class DeleteQuestionMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetQuestionByIdAsync> getQuestion = new();
        private readonly Mock<IUpdateQuestionAsync> update = new();

        private readonly Question question = Question.Create(Guid.NewGuid(), "Hvad spiser en sæl?", null, 1);

        public DeleteQuestionMoqTests()
        {
            getQuestion.Setup(g => g.GetQuestionByIdAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        }

        private DeleteQuestionUseCase CreateUseCase() => new(getQuestion.Object, update.Object);

        [Fact]
        public async Task Delete_is_a_soft_delete_that_keeps_the_row()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteQuestionCommand(question.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.False(question.IsActive);
            update.Verify(u => u.UpdateQuestionAsync(question, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleting_an_already_deleted_question_is_NotFound()
        {
            question.Deactivate();

            var result = await CreateUseCase().DeleteAsync(new DeleteQuestionCommand(question.Id), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_question_is_NotFound()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteQuestionCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
