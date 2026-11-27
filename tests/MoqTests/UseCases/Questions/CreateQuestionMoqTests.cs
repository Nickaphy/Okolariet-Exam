using Application.Commands.QuestionCommands;                // CreateQuestionCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IAssetRepository;            // IAssetExistsAsync
using Application.OutBoundPort.IModdedTemplateRepository;   // ITemplateSupportsQuestionsAsync
using Application.OutBoundPort.IQuestionRepository;         // IAddQuestionAsync
using Application.UseCases.Questions;                       // CreateQuestionUseCase
using Domain.Entities;                                      // Question
using Moq;

namespace MoqTests.UseCases.Questions
{
    public class CreateQuestionMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<ITemplateSupportsQuestionsAsync> supportsQuestions = new();
        private readonly Mock<IAssetExistsAsync> assetExists = new();
        private readonly Mock<IAddQuestionAsync> add = new();

        private readonly Guid quizId = Guid.NewGuid();

        public CreateQuestionMoqTests()
        {
            supportsQuestions.Setup(s => s.SupportsQuestionsAsync(quizId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private CreateQuestionUseCase CreateUseCase() => new(supportsQuestions.Object, assetExists.Object, add.Object);

        private void AssertNothingSaved()
            => add.Verify(a => a.AddQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);

        [Fact]
        public async Task Question_on_a_quiz_template_is_saved()
        {
            Question? saved = null;
            add.Setup(a => a.AddQuestionAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
               .Callback<Question, CancellationToken>((question, _) => saved = question)
               .Returns(Task.CompletedTask);

            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(quizId, "Hvor mange ben har en krabbe?", null, 3), Ct);

            Assert.True(result.IsSuccess);
            Assert.NotNull(saved);
            Assert.Same(result.Value, saved);
            Assert.Equal(quizId, saved.ModdedTemplateId);
            Assert.Equal("Hvor mange ben har en krabbe?", saved.Text);
            Assert.Equal(3, saved.SortOrder);
            Assert.True(saved.IsActive);
            assetExists.Verify(a => a.AssetExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Template_without_questions_is_BusinessRuleError()
        {
            var menuId = Guid.NewGuid();   // SupportsQuestionsAsync returns false for anything but quizId

            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(menuId, "Spørgsmål", null, 1), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Existing_asset_is_attached()
        {
            var assetId = Guid.NewGuid();
            assetExists.Setup(a => a.AssetExistsAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(quizId, "Hvad ser du?", assetId, 1), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(assetId, result.Value.AssetId);
        }

        [Fact]
        public async Task Unknown_asset_is_NotFound()
        {
            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(quizId, "Hvad ser du?", Guid.NewGuid(), 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Text_of_500_characters_is_allowed()
        {
            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(quizId, new string('a', 500), null, 1), Ct);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]   // null -> 501 characters, see below
        public async Task Empty_or_too_long_text_is_BusinessRuleError(string? text)
        {
            text ??= new string('a', 501);

            var result = await CreateUseCase().CreateAsync(new CreateQuestionCommand(quizId, text, null, 1), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }
    }
}
