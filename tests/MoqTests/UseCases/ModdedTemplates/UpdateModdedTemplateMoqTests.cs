using Application.Commands.ModdedTemplateCommands;          // UpdateModdedTemplateCommand, AppearanceInput
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using Application.UseCases.ModdedTemplates;                 // UpdateModdedTemplateUseCase
using Domain.Entities;                                      // ModdedTemplate
using Moq;

namespace MoqTests.UseCases.ModdedTemplates
{
    public class UpdateModdedTemplateMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetModdedTemplateByIdAsync> getModdedTemplate = new();
        private readonly Mock<IUpdateModdedTemplateAsync> update = new();

        private readonly ModdedTemplate moddedTemplate = ModdedTemplate.Create(Guid.NewGuid(), TemplateTestData.Menu(), "Forside", null, 1);

        public UpdateModdedTemplateMoqTests()
        {
            getModdedTemplate.Setup(g => g.GetModdedTemplateByIdAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(moddedTemplate);
        }

        private UpdateModdedTemplateUseCase CreateUseCase() => new(getModdedTemplate.Object, update.Object);

        private static UpdateModdedTemplateCommand Command(Guid id, string title = "Ny titel", AppearanceInput? appearance = null)
            => new(id, title, appearance, 5, "\"etag\"");

        [Fact]
        public async Task Update_changes_title_appearance_and_sort_order_but_not_the_template_type()
        {
            var result = await CreateUseCase().UpdateAsync(Command(moddedTemplate.Id, appearance: new AppearanceInput("#82afbb", null, null, null)), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ny titel", moddedTemplate.Title);
            Assert.Equal("#82afbb", moddedTemplate.Appearance.BackgroundColor);
            Assert.Equal(5, moddedTemplate.SortOrder);
            Assert.Equal(TemplateTestData.MenuType, moddedTemplate.TemplateType);
            update.Verify(u => u.UpdateModdedTemplateAsync(moddedTemplate, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Unknown_modded_template_is_NotFound()
        {
            var result = await CreateUseCase().UpdateAsync(Command(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Invalid_colour_is_BusinessRuleError_and_nothing_changes()
        {
            var result = await CreateUseCase().UpdateAsync(Command(moddedTemplate.Id, appearance: new AppearanceInput("blue", null, null, null)), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal("Forside", moddedTemplate.Title);
            update.Verify(u => u.UpdateModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Empty_title_is_BusinessRuleError_and_nothing_changes()
        {
            var result = await CreateUseCase().UpdateAsync(Command(moddedTemplate.Id, title: ""), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal("Forside", moddedTemplate.Title);
            update.Verify(u => u.UpdateModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
