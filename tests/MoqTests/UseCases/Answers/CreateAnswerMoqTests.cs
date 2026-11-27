using Application.Commands.AnswerCommands;             // CreateAnswerCommand
using Application.ErrorMessages;                       // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IAssetRepository;       // IAssetExistsAsync
using Application.OutBoundPort.IQuestionRepository;    // IGetQuestionByIdAsync, IUpdateQuestionAsync
using Application.UseCases.Answers;                    // CreateAnswerUseCase
using Domain.Entities;                                 // Question
using Moq;

namespace MoqTests.UseCases.Answers
{
    // An answer only exists inside its question, so it is saved by saving the question.
    public class CreateAnswerMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetQuestionByIdAsync> getQuestion = new();
        private readonly Mock<IAssetExistsAsync> assetExists = new();
        private readonly Mock<IUpdateQuestionAsync> update = new();

        private readonly Question question = Question.Create(Guid.NewGuid(), "Hvad spiser en sæl?", null, 1);

        public CreateAnswerMoqTests()
        {
            getQuestion.Setup(g => g.GetQuestionByIdAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        }

        private CreateAnswerUseCase CreateUseCase() => new(getQuestion.Object, assetExists.Object, update.Object);

        private void AssertNothingAdded()
        {
            Assert.Empty(question.Answers);
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Answer_is_added_to_the_question_and_saved_through_it()
        {
            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(question.Id, "Fisk", null, true, 1), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(result.Value, Assert.Single(question.Answers));
            Assert.Equal(question.Id, result.Value.QuestionId);
            Assert.Equal("Fisk", result.Value.Text);
            Assert.True(result.Value.IsCorrect);
            update.Verify(u => u.UpdateQuestionAsync(question, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleted_question_is_NotFound()
        {
            question.Deactivate();

            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(question.Id, "Fisk", null, true, 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingAdded();
        }

        [Fact]
        public async Task Unknown_question_is_NotFound()
        {
            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(Guid.NewGuid(), "Fisk", null, true, 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingAdded();
        }

        [Fact]
        public async Task Unknown_asset_is_NotFound()
        {
            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(question.Id, "Fisk", Guid.NewGuid(), true, 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingAdded();
        }

        [Fact]
        public async Task Text_of_200_characters_is_allowed()
        {
            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(question.Id, new string('a', 200), null, false, 1), Ct);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]   // null -> 201 characters, see below
        public async Task Empty_or_too_long_text_is_BusinessRuleError(string? text)
        {
            text ??= new string('a', 201);

            var result = await CreateUseCase().CreateAsync(new CreateAnswerCommand(question.Id, text, null, false, 1), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingAdded();
        }
    }
}
