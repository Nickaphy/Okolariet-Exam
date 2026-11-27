using Application.Commands.AnswerCommands;             // UpdateAnswerCommand
using Application.ErrorMessages;                       // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IAssetRepository;       // IAssetExistsAsync
using Application.OutBoundPort.IQuestionRepository;    // IGetQuestionByAnswerIdAsync, IUpdateQuestionAsync
using Application.UseCases.Answers;                    // UpdateAnswerUseCase
using Domain.Entities;                                 // Question, Answer
using Moq;

namespace MoqTests.UseCases.Answers
{
    public class UpdateAnswerMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetQuestionByAnswerIdAsync> getQuestionByAnswer = new();
        private readonly Mock<IAssetExistsAsync> assetExists = new();
        private readonly Mock<IUpdateQuestionAsync> update = new();

        private readonly Question question = Question.Create(Guid.NewGuid(), "Hvad spiser en sæl?", null, 1);
        private readonly Answer answer;

        public UpdateAnswerMoqTests()
        {
            answer = question.AddAnswer("Fisk", true, null, 1);
            getQuestionByAnswer.Setup(g => g.GetQuestionByAnswerIdAsync(answer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        }

        private UpdateAnswerUseCase CreateUseCase() => new(getQuestionByAnswer.Object, assetExists.Object, update.Object);

        private void AssertUnchanged()
        {
            Assert.Equal("Fisk", answer.Text);
            Assert.True(answer.IsCorrect);
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Update_changes_the_answer_and_saves_the_question()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(answer.Id, "Tang", null, false, 2), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(answer, result.Value);
            Assert.Equal("Tang", answer.Text);
            Assert.False(answer.IsCorrect);
            Assert.Equal(2, answer.SortOrder);
            update.Verify(u => u.UpdateQuestionAsync(question, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleted_answer_is_NotFound()
        {
            answer.Deactivate();

            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(answer.Id, "Tang", null, false, 2), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Unknown_answer_is_NotFound()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(Guid.NewGuid(), "Tang", null, false, 2), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Question_that_does_not_contain_the_answer_is_NotFound()
        {
            // The repository returned a question, but the answer id isn't one of its answers.
            var otherAnswerId = Guid.NewGuid();
            getQuestionByAnswer.Setup(g => g.GetQuestionByAnswerIdAsync(otherAnswerId, It.IsAny<CancellationToken>())).ReturnsAsync(question);

            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(otherAnswerId, "Tang", null, false, 2), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Unknown_asset_is_NotFound_and_the_answer_is_unchanged()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(answer.Id, "Tang", Guid.NewGuid(), false, 2), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Too_long_text_is_BusinessRuleError_and_the_answer_is_unchanged()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateAnswerCommand(answer.Id, new string('a', 201), null, false, 2), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertUnchanged();
        }
    }
}
