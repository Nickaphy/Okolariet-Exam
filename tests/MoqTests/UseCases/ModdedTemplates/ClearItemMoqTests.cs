using Application.Commands.ModdedTemplateCommands;          // ClearItemCommand
using Application.ErrorMessages;                            // NotFoundError
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using Application.UseCases.ModdedTemplates;                 // ClearItemUseCase
using Domain.Entities;                                      // ModdedTemplate, Template
using Moq;

namespace MoqTests.UseCases.ModdedTemplates
{
    public class ClearItemMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetModdedTemplateByIdAsync> getModdedTemplate = new();
        private readonly Mock<IUpdateModdedTemplateAsync> update = new();

        private readonly Template template = TemplateTestData.Menu();
        private readonly ModdedTemplate moddedTemplate;

        public ClearItemMoqTests()
        {
            moddedTemplate = ModdedTemplate.Create(Guid.NewGuid(), template, "Forside", null, 1);
            getModdedTemplate.Setup(g => g.GetModdedTemplateByIdAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(moddedTemplate);
        }

        private ClearItemUseCase CreateUseCase() => new(getModdedTemplate.Object, update.Object);

        [Fact]
        public async Task Clear_removes_the_slots_content_and_saves()
        {
            moddedTemplate.SetItem(template, "title", "Velkommen", null, null);

            var result = await CreateUseCase().ClearItemAsync(new ClearItemCommand(moddedTemplate.Id, "title"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(moddedTemplate.Items);
            update.Verify(u => u.UpdateModdedTemplateAsync(moddedTemplate, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Clear_only_removes_the_given_slot()
        {
            moddedTemplate.SetItem(template, "title", "Velkommen", null, null);
            moddedTemplate.SetItem(template, "button1", "Start", null, null);

            await CreateUseCase().ClearItemAsync(new ClearItemCommand(moddedTemplate.Id, "title"), Ct);

            Assert.Equal("button1", Assert.Single(moddedTemplate.Items).SlotKey);
        }

        [Fact]
        public async Task Clearing_an_empty_slot_is_fine()
        {
            var result = await CreateUseCase().ClearItemAsync(new ClearItemCommand(moddedTemplate.Id, "title"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(moddedTemplate.Items);
        }

        [Fact]
        public async Task Unknown_modded_template_is_NotFound()
        {
            var result = await CreateUseCase().ClearItemAsync(new ClearItemCommand(Guid.NewGuid(), "title"), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
