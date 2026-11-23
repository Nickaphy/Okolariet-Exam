using Application.Commands.ModdedTemplateCommands;   // the modded template commands
using Application.ErrorMessages;                     // NotFoundError, ConflictError, BusinessRuleError
using Application.UseCases.ModdedTemplates;          // the modded template use cases
using Domain.ValueObjects;                           // Appearance
using xUnitTests.ApplicationTests.Fakes;             // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class ModdedTemplateUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeProjectRepository projects = new();
        private readonly FakeModdedTemplateRepository moddedTemplates = new();
        private readonly FakeTemplateCatalog catalog = new();
        private readonly FakeQuestionRepository questions = new();
        private readonly Guid projectId = Guid.NewGuid();

        public ModdedTemplateUseCaseTests()
        {
            projects.Projects.Add(projectId);   // xUnit makes a new instance per test, so every test starts with one project
        }

        // ---------- List ----------

        [Fact]
        public async Task List_returns_only_this_projects_templates_sorted_by_SortOrder()
        {
            var second = TestData.MenuModdedTemplate(projectId, sortOrder: 2);
            var first = TestData.MenuModdedTemplate(projectId, sortOrder: 1);
            moddedTemplates.ModdedTemplates.AddRange([second, first, TestData.MenuModdedTemplate()]);   // last one is another project
            var useCase = new ListModdedTemplatesUseCase(projects, moddedTemplates);

            var result = await useCase.ListAsync(projectId, Ct);

            Assert.Equal([first, second], result.Value);
        }

        [Fact]
        public async Task List_for_existing_project_without_templates_is_empty_not_404()
        {
            var result = await new ListModdedTemplatesUseCase(projects, moddedTemplates).ListAsync(projectId, Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task List_for_unknown_project_is_NotFound()
        {
            var result = await new ListModdedTemplatesUseCase(projects, moddedTemplates).ListAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // ---------- Get ----------

        [Fact]
        public async Task Get_returns_template_with_active_questions_sorted()
        {
            var mt = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(mt);
            var q2 = TestData.Question(mt.Id, sortOrder: 2);
            var q1 = TestData.Question(mt.Id, sortOrder: 1);
            var deleted = TestData.Question(mt.Id, sortOrder: 0);
            deleted.Deactivate();
            questions.Questions.AddRange([q2, q1, deleted, TestData.Question()]);
            var useCase = new GetModdedTemplateUseCase(moddedTemplates, questions);

            var result = await useCase.GetAsync(mt.Id, Ct);

            Assert.Same(mt, result.Value.ModdedTemplate);
            Assert.Equal([q1, q2], result.Value.Questions);
        }

        [Fact]
        public async Task Get_unknown_is_NotFound()
        {
            var result = await new GetModdedTemplateUseCase(moddedTemplates, questions).GetAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // ---------- Create ----------

        [Fact]
        public async Task Create_saves_a_new_modded_template()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);
            var appearance = new AppearanceInput("#0A6E8A", null, null, "Nunito");

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(projectId, TestData.MenuType, "Forside", appearance, 1), Ct);

            Assert.True(result.IsSuccess);
            var saved = Assert.Single(moddedTemplates.ModdedTemplates);
            Assert.Same(saved, result.Value);
            Assert.Equal(projectId, saved.ProjectId);
            Assert.Equal(TestData.MenuType, saved.TemplateType);
            Assert.Equal("#0A6E8A", saved.Appearance.BackgroundColor);
        }

        [Fact]
        public async Task Create_without_appearance_uses_Empty()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(projectId, TestData.MenuType, "Forside", null, 0), Ct);

            Assert.Equal(Appearance.Empty, result.Value.Appearance);
        }

        [Fact]
        public async Task Create_in_unknown_project_is_NotFound()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(Guid.NewGuid(), TestData.MenuType, "Forside", null, 0), Ct);

            Assert.True(result.HasError<NotFoundError>());
            Assert.Empty(moddedTemplates.ModdedTemplates);
        }

        [Fact]
        public async Task Create_with_unknown_template_type_is_BusinessRuleError()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(projectId, "does-not-exist", "Forside", null, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(moddedTemplates.ModdedTemplates);
        }

        [Fact]
        public async Task Create_with_bad_colour_is_BusinessRuleError_not_an_exception()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);
            var badColour = new AppearanceInput("blue", null, null, null);

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(projectId, TestData.MenuType, "Forside", badColour, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(moddedTemplates.ModdedTemplates);
        }

        [Fact]
        public async Task Create_with_blank_title_is_BusinessRuleError()
        {
            var useCase = new CreateModdedTemplateUseCase(projects, catalog, moddedTemplates);

            var result = await useCase.CreateAsync(new CreateModdedTemplateCommand(projectId, TestData.MenuType, " ", null, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        // ---------- Update ----------

        [Fact]
        public async Task Update_changes_and_saves()
        {
            var mt = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(mt);
            var useCase = new UpdateModdedTemplateUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.UpdateAsync(
                new UpdateModdedTemplateCommand(mt.Id, "Ny titel", new AppearanceInput(null, "#FF0000", null, null), 3, "\"1\""), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ny titel", mt.Title);
            Assert.Equal("#FF0000", mt.Appearance.ButtonColor);
            Assert.Equal(3, mt.SortOrder);
            Assert.Equal(1, moddedTemplates.UpdateCount);
        }

        [Fact]
        public async Task Update_unknown_is_NotFound()
        {
            var useCase = new UpdateModdedTemplateUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.UpdateAsync(new UpdateModdedTemplateCommand(Guid.NewGuid(), "Titel", null, 0, "\"1\""), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Update_with_bad_colour_is_BusinessRuleError_and_not_saved()
        {
            var mt = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(mt);
            var useCase = new UpdateModdedTemplateUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.UpdateAsync(
                new UpdateModdedTemplateCommand(mt.Id, "Ny titel", new AppearanceInput("nope", null, null, null), 0, "\"1\""), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal("Forside", mt.Title);
            Assert.Equal(0, moddedTemplates.UpdateCount);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_removes_the_modded_template()
        {
            var mt = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(mt);
            var useCase = new DeleteModdedTemplateUseCase(moddedTemplates, projects, moddedTemplates, moddedTemplates);

            var result = await useCase.DeleteAsync(new DeleteModdedTemplateCommand(mt.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(moddedTemplates.ModdedTemplates);
        }

        [Fact]
        public async Task Delete_unknown_is_NotFound()
        {
            var useCase = new DeleteModdedTemplateUseCase(moddedTemplates, projects, moddedTemplates, moddedTemplates);

            var result = await useCase.DeleteAsync(new DeleteModdedTemplateCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Delete_start_screen_is_Conflict()
        {
            var mt = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(mt);
            projects.MainModdedTemplateIds.Add(mt.Id);
            var useCase = new DeleteModdedTemplateUseCase(moddedTemplates, projects, moddedTemplates, moddedTemplates);

            var result = await useCase.DeleteAsync(new DeleteModdedTemplateCommand(mt.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            Assert.Single(moddedTemplates.ModdedTemplates);
        }

        [Fact]
        public async Task Delete_when_a_button_on_another_template_links_here_is_Conflict()
        {
            var menu = TestData.MenuModdedTemplate(projectId);
            var target = TestData.MenuModdedTemplate(projectId);
            menu.SetItem(TestData.MenuTemplate(), "button1", "Gå videre", null, target.Id);
            moddedTemplates.ModdedTemplates.AddRange([menu, target]);
            var useCase = new DeleteModdedTemplateUseCase(moddedTemplates, projects, moddedTemplates, moddedTemplates);

            var result = await useCase.DeleteAsync(new DeleteModdedTemplateCommand(target.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            Assert.Equal(2, moddedTemplates.ModdedTemplates.Count);
        }

        [Fact]
        public async Task Delete_template_that_links_elsewhere_is_allowed()
        {
            // Only INCOMING links block a delete. Outgoing links just disappear with it.
            var menu = TestData.MenuModdedTemplate(projectId);
            var target = TestData.MenuModdedTemplate(projectId);
            menu.SetItem(TestData.MenuTemplate(), "button1", "Gå videre", null, target.Id);
            moddedTemplates.ModdedTemplates.AddRange([menu, target]);
            var useCase = new DeleteModdedTemplateUseCase(moddedTemplates, projects, moddedTemplates, moddedTemplates);

            var result = await useCase.DeleteAsync(new DeleteModdedTemplateCommand(menu.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(target, Assert.Single(moddedTemplates.ModdedTemplates));
        }
    }
}
