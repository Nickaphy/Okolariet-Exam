using Application.Commands.QuestionCommands;           // UpdateQuestionCommand
using Application.ErrorMessages;                       // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IAssetRepository;       // IAssetExistsAsync
using Application.OutBoundPort.IQuestionRepository;    // IGetQuestionByIdAsync, IUpdateQuestionAsync
using Application.UseCases.Questions;                  // UpdateQuestionUseCase
using Domain.Entities;                                 // Question
using Moq;

namespace MoqTests.UseCases.Questions
{
    public class UpdateQuestionMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetQuestionByIdAsync> getQuestion = new();
        private readonly Mock<IAssetExistsAsync> assetExists = new();
        private readonly Mock<IUpdateQuestionAsync> update = new();

        private readonly Question question = Question.Create(Guid.NewGuid(), "Gammel tekst", null, 1);

        public UpdateQuestionMoqTests()
        {
            getQuestion.Setup(g => g.GetQuestionByIdAsync(question.Id, It.IsAny<CancellationToken>())).ReturnsAsync(question);
        }

        private UpdateQuestionUseCase CreateUseCase() => new(getQuestion.Object, assetExists.Object, update.Object);

        private static UpdateQuestionCommand Command(Guid id, string text = "Ny tekst", Guid? assetId = null, int sortOrder = 2)
            => new(id, text, assetId, sortOrder, "\"etag\"");

        private void AssertUnchanged()
        {
            Assert.Equal("Gammel tekst", question.Text);
            Assert.Null(question.AssetId);
            Assert.Equal(1, question.SortOrder);
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Update_changes_text_asset_and_sort_order()
        {
            var assetId = Guid.NewGuid();
            assetExists.Setup(a => a.AssetExistsAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().UpdateAsync(Command(question.Id, "Ny tekst", assetId, 2), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ny tekst", question.Text);
            Assert.Equal(assetId, question.AssetId);
            Assert.Equal(2, question.SortOrder);
            update.Verify(u => u.UpdateQuestionAsync(question, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleted_question_is_NotFound()
        {
            question.Deactivate();

            var result = await CreateUseCase().UpdateAsync(Command(question.Id), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Unknown_question_is_NotFound()
        {
            var result = await CreateUseCase().UpdateAsync(Command(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_asset_is_NotFound_and_the_question_is_unchanged()
        {
            var result = await CreateUseCase().UpdateAsync(Command(question.Id, assetId: Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Empty_text_is_BusinessRuleError_and_the_question_is_unchanged()
        {
            var result = await CreateUseCase().UpdateAsync(Command(question.Id, text: ""), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertUnchanged();
        }
    }
}
