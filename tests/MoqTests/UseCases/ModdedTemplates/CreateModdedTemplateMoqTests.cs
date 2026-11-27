using Application.Commands.ModdedTemplateCommands;          // CreateModdedTemplateCommand, AppearanceInput
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IModdedTemplateRepository;   // IAddModdedTemplateAsync
using Application.OutBoundPort.IProjectRepository;          // IProjectExistsAsync
using Application.OutBoundPort.ITemplateRepository;         // IGetTemplateByTypeAsync
using Application.UseCases.ModdedTemplates;                 // CreateModdedTemplateUseCase
using Domain.Entities;                                      // ModdedTemplate
using Domain.ValueObjects;                                  // Appearance
using Moq;

namespace MoqTests.UseCases.ModdedTemplates
{
    public class CreateModdedTemplateMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IProjectExistsAsync> projectExists = new();
        private readonly Mock<IGetTemplateByTypeAsync> getTemplate = new();
        private readonly Mock<IAddModdedTemplateAsync> add = new();

        private readonly Guid projectId = Guid.NewGuid();

        public CreateModdedTemplateMoqTests()
        {
            projectExists.Setup(p => p.ProjectExistsAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            getTemplate.Setup(g => g.GetTemplateByTypeAsync(TemplateTestData.MenuType, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(TemplateTestData.Menu());
        }

        private CreateModdedTemplateUseCase CreateUseCase() => new(projectExists.Object, getTemplate.Object, add.Object);

        private static CreateModdedTemplateCommand Command(Guid projectId, string type = TemplateTestData.MenuType,
                                                           string title = "Forside", AppearanceInput? appearance = null)
            => new(projectId, type, title, appearance, 1);

        private void AssertNothingSaved()
            => add.Verify(a => a.AddModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);

        [Fact]
        public async Task Create_from_a_known_template_is_saved()
        {
            var result = await CreateUseCase().CreateAsync(Command(projectId), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(projectId, result.Value.ProjectId);
            Assert.Equal(TemplateTestData.MenuType, result.Value.TemplateType);
            Assert.Equal("Forside", result.Value.Title);
            Assert.Equal(Appearance.Empty, result.Value.Appearance);
            add.Verify(a => a.AddModdedTemplateAsync(result.Value, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Valid_colours_and_font_are_kept()
        {
            var appearance = new AppearanceInput("#82afbb", "#FFFFFF", "#000000", "Arial");

            var result = await CreateUseCase().CreateAsync(Command(projectId, appearance: appearance), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("#82afbb", result.Value.Appearance.BackgroundColor);
            Assert.Equal("Arial", result.Value.Appearance.FontFamily);
        }

        [Fact]
        public async Task Unknown_project_is_NotFound()
        {
            var result = await CreateUseCase().CreateAsync(Command(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            getTemplate.Verify(g => g.GetTemplateByTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unknown_template_type_is_BusinessRuleError()
        {
            var result = await CreateUseCase().CreateAsync(Command(projectId, type: "does-not-exist"), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Theory]
        [InlineData("blue")]
        [InlineData("#12345")]
        [InlineData("82afbb")]
        [InlineData("#82afbg")]
        public async Task Invalid_colour_is_BusinessRuleError(string colour)
        {
            var result = await CreateUseCase().CreateAsync(Command(projectId, appearance: new AppearanceInput(colour, null, null, null)), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Title_of_120_characters_is_allowed()
        {
            var result = await CreateUseCase().CreateAsync(Command(projectId, title: new string('a', 120)), Ct);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]   // null -> 121 characters, see below
        public async Task Empty_or_too_long_title_is_BusinessRuleError(string? title)
        {
            title ??= new string('a', 121);

            var result = await CreateUseCase().CreateAsync(Command(projectId, title: title), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }
    }
}
