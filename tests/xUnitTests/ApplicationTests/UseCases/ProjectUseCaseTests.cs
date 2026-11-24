using Application.Commands.ProjectCommands;   // CreateProjectCommand, UpdateProjectCommand, DeleteProjectCommand
using Application.ErrorMessages;              // NotFoundError, BusinessRuleError, UnexpectedError
using Application.UseCases.Projects;          // CreateProjectUseCase, UpdateProjectUseCase
using Domain.Entities;                        // Project
using Domain.ValueObjects;                    // Appearance
using xUnitTests.ApplicationTests.Fakes;      // FakeProjectStore

namespace xUnitTests.ApplicationTests.UseCases
{
    // ListProjectsUseCase, GetProjectUseCase and DeleteProjectUseCase live in MoreProjectUseCases.cs without a namespace,
    // so they need no using.
    public class ProjectUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private readonly FakeProjectStore store = new();

        private Project AddProject(string name = "Havet")
        {
            var project = Project.Create(name);
            store.Projects.Add(project);
            return project;
        }

        private UpdateProjectUseCase UpdateUseCase() => new(store, store, store);

        // Create

        [Fact]
        public async Task Create_saves_and_returns_the_project()
        {
            var result = await new CreateProjectUseCase(store).CreateAsync(new CreateProjectCommand("Havet", null, null), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Havet", result.Value.Name);
            Assert.Contains(result.Value, store.Projects);
        }

        [Fact]
        public async Task Create_with_start_screen_is_BusinessRuleError_and_saves_nothing()
        {
            // A new project has no screens yet, so it cannot have a start screen.
            var result = await new CreateProjectUseCase(store).CreateAsync(new CreateProjectCommand("Havet", Guid.NewGuid(), null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(store.Projects);
        }

        [Fact]
        public async Task Create_with_invalid_name_is_BusinessRuleError_and_saves_nothing()
        {
            var result = await new CreateProjectUseCase(store).CreateAsync(new CreateProjectCommand("", null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(store.Projects);
        }

        [Fact]
        public async Task Create_when_saving_fails_is_UnexpectedError()
        {
            store.FailOnSave = true;

            var result = await new CreateProjectUseCase(store).CreateAsync(new CreateProjectCommand("Havet", null, null), Ct);

            Assert.True(result.HasError<UnexpectedError>());
        }

        // Update

        [Fact]
        public async Task Update_changes_and_saves_the_project()
        {
            var project = AddProject();
            var appearance = new Appearance("#0A6E8A", null, null, null);

            var result = await UpdateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "Skoven", null, appearance), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Skoven", project.Name);
            Assert.Equal(appearance, project.Appearance);
            Assert.Equal(1, store.UpdateCount);
        }

        [Fact]
        public async Task Update_unknown_project_is_NotFound()
        {
            var result = await UpdateUseCase().UpdateAsync(new UpdateProjectCommand(Guid.NewGuid(), "Skoven", null, null), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Update_with_start_screen_from_this_project_sets_it()
        {
            var project = AddProject();
            var screen = Guid.NewGuid();
            store.Screens.Add((screen, project.Id));

            var result = await UpdateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "Havet", screen, null), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(screen, project.MainModdedTemplateId);
        }

        [Fact]
        public async Task Update_with_start_screen_from_another_project_is_BusinessRuleError_and_saves_nothing()
        {
            var project = AddProject();
            var screen = Guid.NewGuid();
            store.Screens.Add((screen, Guid.NewGuid()));   // the screen exists, but in another project

            var result = await UpdateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "Havet", screen, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Null(project.MainModdedTemplateId);
            Assert.Equal(0, store.UpdateCount);
        }

        [Fact]
        public async Task Update_with_invalid_name_is_BusinessRuleError_and_keeps_old_name()
        {
            var project = AddProject();

            var result = await UpdateUseCase().UpdateAsync(new UpdateProjectCommand(project.Id, "", null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal("Havet", project.Name);
            Assert.Equal(0, store.UpdateCount);
        }

        // Get

        [Fact]
        public async Task Get_returns_the_project()
        {
            var project = AddProject();

            var result = await new GetProjectUseCase(store).GetAsync(project.Id, Ct);

            Assert.Same(project, result.Value);
        }

        [Fact]
        public async Task Get_unknown_is_NotFound()
        {
            var result = await new GetProjectUseCase(store).GetAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // List

        [Fact]
        public async Task List_returns_the_requested_page_and_the_total_count()
        {
            AddProject("C");
            AddProject("A");
            AddProject("B");

            var result = await new ListProjectsUseCase(store).ListAsync(page: 2, pageSize: 2, Ct);

            Assert.Equal(new[] { "C" }, result.Value.Items.Select(p => p.Name));
            Assert.Equal(2, result.Value.Page);
            Assert.Equal(3, result.Value.TotalCount);
        }

        // Delete

        [Fact]
        public async Task Delete_removes_the_project()
        {
            var project = AddProject();

            var result = await new DeleteProjectUseCase(store, store).DeleteAsync(new DeleteProjectCommand(project.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(store.Projects);
        }

        [Fact]
        public async Task Delete_unknown_is_NotFound()
        {
            var result = await new DeleteProjectUseCase(store, store).DeleteAsync(new DeleteProjectCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }
    }
}
