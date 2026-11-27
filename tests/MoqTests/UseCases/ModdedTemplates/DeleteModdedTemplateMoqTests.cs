using Application.Commands.ModdedTemplateCommands;          // DeleteModdedTemplateCommand
using Application.ErrorMessages;                            // NotFoundError, ConflictError
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IIsLinkedFromButtonAsync, IDeleteModdedTemplateAsync
using Application.OutBoundPort.IProjectRepository;          // IIsMainModdedTemplateAsync
using Application.UseCases.ModdedTemplates;                 // DeleteModdedTemplateUseCase
using Domain.Entities;                                      // ModdedTemplate
using Moq;

namespace MoqTests.UseCases.ModdedTemplates
{
    // Spec (contracts/openapi.yaml, DELETE /modded-templates/{id}):
    // blocked with 409 if a button on another modded template links to it, or if it is the project's start screen.
    public class DeleteModdedTemplateMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetModdedTemplateByIdAsync> getModdedTemplate = new();
        private readonly Mock<IIsMainModdedTemplateAsync> isMain = new();
        private readonly Mock<IIsLinkedFromButtonAsync> isLinked = new();
        private readonly Mock<IDeleteModdedTemplateAsync> delete = new();

        private readonly ModdedTemplate moddedTemplate = ModdedTemplate.Create(Guid.NewGuid(), TemplateTestData.Menu(), "Underside", null, 2);

        public DeleteModdedTemplateMoqTests()
        {
            getModdedTemplate.Setup(g => g.GetModdedTemplateByIdAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(moddedTemplate);
        }

        private DeleteModdedTemplateUseCase CreateUseCase() => new(getModdedTemplate.Object, isMain.Object, isLinked.Object, delete.Object);

        private void AssertNotDeleted()
            => delete.Verify(d => d.DeleteModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);

        [Fact]
        public async Task Unused_modded_template_is_deleted()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteModdedTemplateCommand(moddedTemplate.Id), Ct);

            Assert.True(result.IsSuccess);
            delete.Verify(d => d.DeleteModdedTemplateAsync(moddedTemplate, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Start_screen_is_Conflict()
        {
            isMain.Setup(i => i.IsMainModdedTemplateAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().DeleteAsync(new DeleteModdedTemplateCommand(moddedTemplate.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            AssertNotDeleted();
        }

        [Fact]
        public async Task Target_of_a_button_is_Conflict()
        {
            isLinked.Setup(i => i.IsLinkedFromButtonAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().DeleteAsync(new DeleteModdedTemplateCommand(moddedTemplate.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            AssertNotDeleted();
        }

        [Fact]
        public async Task Unknown_modded_template_is_NotFound()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteModdedTemplateCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNotDeleted();
        }
    }
}
