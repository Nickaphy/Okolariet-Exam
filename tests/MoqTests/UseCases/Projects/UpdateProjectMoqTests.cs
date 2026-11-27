using Application.Commands.ProjectCommands;            // UpdateProjectCommand
using Application.ErrorMessages;                       // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IProjectRepository;     // IGetProjectByIdAsync, IModdedTemplateBelongsToProjectAsync, IUpdateProjectAsync
using Application.UseCases.Projects;                   // UpdateProjectUseCase
using Domain.Entities;                                 // Project
using Moq;

namespace MoqTests.UseCases.Projects
{
    public class UpdateProjectMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetProjectByIdAsync> getById = new();
        private readonly Mock<IModdedTemplateBelongsToProjectAsync> belongs = new();
        private readonly Mock<IUpdateProjectAsync> update = new();

        private readonly Project project = Project.Create("Havets Hus");

        public UpdateProjectMoqTests()
        {
            getById.Setup(g => g.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        }

        private UpdateProjectUseCase CreateUseCase() => new(getById.Object, belongs.Object, update.Object);

        private void AssertUnchanged()
        {
            Assert.Equal("Havets Hus", project.Name);
            Assert.Null(project.MainModdedTemplateId);
            update.Verify(u => u.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Update_with_a_start_screen_from_this_project_is_saved()
        {
            var startScreen = Guid.NewGuid();
            belongs.Setup(b => b.BelongsToProjectAsync(startScreen, project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, " Nyt navn ", startScreen, null), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nyt navn", project.Name);
            Assert.Equal(startScreen, project.MainModdedTemplateId);
            update.Verify(u => u.UpdateAsync(project, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Start_screen_from_another_project_is_BusinessRuleError()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "Nyt navn", Guid.NewGuid(), null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertUnchanged();
        }

        [Fact]
        public async Task Without_a_start_screen_nothing_is_checked()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "Nyt navn", null, null), Ct);

            Assert.True(result.IsSuccess);
            belongs.Verify(b => b.BelongsToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_project_is_NotFound()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateProjectCommand(Guid.NewGuid(), "Nyt navn", null, null), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Empty_name_is_BusinessRuleError_and_nothing_changes()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "  ", null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertUnchanged();
        }
    }
}
