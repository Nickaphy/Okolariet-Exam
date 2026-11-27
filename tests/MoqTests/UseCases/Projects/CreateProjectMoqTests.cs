using Application.Commands.ProjectCommands;            // CreateProjectCommand
using Application.ErrorMessages;                       // BusinessRuleError, UnexpectedError
using Application.OutBoundPort.IProjectRepository;     // IAddProjectAsync
using Application.UseCases.Projects;                   // CreateProjectUseCase
using Domain.Entities;                                 // Project
using Moq;

namespace MoqTests.UseCases.Projects
{
    public class CreateProjectMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IAddProjectAsync> add = new();

        private CreateProjectUseCase CreateUseCase() => new(add.Object);

        private void AssertNothingSaved()
            => add.Verify(a => a.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);

        [Fact]
        public async Task Create_trims_the_name_and_saves_the_project()
        {
            var result = await CreateUseCase().CreateAsync(new CreateProjectCommand("  Havets Hus  ", null, null), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Havets Hus", result.Value.Name);
            Assert.Null(result.Value.MainModdedTemplateId);
            add.Verify(a => a.AddAsync(result.Value, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task New_project_cannot_have_a_start_screen_yet()
        {
            var result = await CreateUseCase().CreateAsync(new CreateProjectCommand("Havets Hus", Guid.NewGuid(), null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Name_of_120_characters_is_allowed()
        {
            var result = await CreateUseCase().CreateAsync(new CreateProjectCommand(new string('a', 120), null, null), Ct);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]   // null -> 121 characters, see below
        public async Task Empty_or_too_long_name_is_BusinessRuleError(string? name)
        {
            name ??= new string('a', 121);

            var result = await CreateUseCase().CreateAsync(new CreateProjectCommand(name, null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Database_error_becomes_UnexpectedError_without_leaking_details()
        {
            add.Setup(a => a.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("Server=db;Password=secret"));

            var result = await CreateUseCase().CreateAsync(new CreateProjectCommand("Havets Hus", null, null), Ct);

            Assert.True(result.HasError<UnexpectedError>());
            Assert.DoesNotContain(result.Errors, e => e.Message.Contains("secret"));
        }
    }
}
