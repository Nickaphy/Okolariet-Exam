using Application.Commands.QuestionCommands;   // CreateQuestionCommand, UpdateQuestionCommand, DeleteQuestionCommand
using Application.ErrorMessages;               // NotFoundError, BusinessRuleError
using Application.UseCases.Questions;          // the question use cases
using xUnitTests.ApplicationTests.Fakes;       // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class QuestionUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeQuestionRepository questions = new();
        private readonly FakeAssetRepository assets = new();
        private readonly FakeModdedTemplateRepository moddedTemplates = new();

        // ---------- Create ----------

        [Fact]
        public async Task Create_on_quiz_saves_question()
        {
            var quizId = Guid.NewGuid();
            moddedTemplates.SupportsQuestions.Add(quizId);
            var useCase = new CreateQuestionUseCase(moddedTemplates, assets, questions);

            var result = await useCase.CreateAsync(new CreateQuestionCommand(quizId, "Hvor dybt er havet?", null, 0), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(result.Value, Assert.Single(questions.Questions));
        }

        [Fact]
        public async Task Create_on_template_without_questions_is_BusinessRuleError()
        {
            var useCase = new CreateQuestionUseCase(moddedTemplates, assets, questions);

            var result = await useCase.CreateAsync(new CreateQuestionCommand(Guid.NewGuid(), "Hej?", null, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(questions.Questions);
        }

        [Fact]
        public async Task Create_with_unknown_asset_is_NotFound()
        {
            var quizId = Guid.NewGuid();
            moddedTemplates.SupportsQuestions.Add(quizId);
            var useCase = new CreateQuestionUseCase(moddedTemplates, assets, questions);

            var result = await useCase.CreateAsync(new CreateQuestionCommand(quizId, "Hej?", Guid.NewGuid(), 0), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Create_with_blank_text_is_BusinessRuleError()
        {
            var quizId = Guid.NewGuid();
            moddedTemplates.SupportsQuestions.Add(quizId);
            var useCase = new CreateQuestionUseCase(moddedTemplates, assets, questions);

            var result = await useCase.CreateAsync(new CreateQuestionCommand(quizId, "", null, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(questions.Questions);
        }

        // ---------- Get / List ----------

        [Fact]
        public async Task Get_returns_active_question()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);

            var result = await new GetQuestionUseCase(questions).GetAsync(question.Id, Ct);

            Assert.Same(question, result.Value);
        }

        [Fact]
        public async Task Get_deactivated_question_is_NotFound()
        {
            var question = TestData.Question();
            question.Deactivate();
            questions.Questions.Add(question);

            var result = await new GetQuestionUseCase(questions).GetAsync(question.Id, Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task List_hides_deactivated_questions_and_other_templates()
        {
            var quizId = Guid.NewGuid();
            var active = TestData.Question(quizId);
            var deleted = TestData.Question(quizId);
            deleted.Deactivate();
            questions.Questions.AddRange([active, deleted, TestData.Question()]);

            var result = await new ListQuestionsUseCase(questions).ListAsync(quizId, Ct);

            Assert.Same(active, Assert.Single(result.Value));
        }

        // ---------- Update ----------

        [Fact]
        public async Task Update_changes_and_saves()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new UpdateQuestionUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateQuestionCommand(question.Id, "Ny tekst", null, 4, "\"1\""), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ny tekst", question.Text);
            Assert.Equal(1, questions.UpdateCount);
        }

        [Fact]
        public async Task Update_unknown_question_is_NotFound()
        {
            var useCase = new UpdateQuestionUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateQuestionCommand(Guid.NewGuid(), "Tekst", null, 0, "\"1\""), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Update_with_invalid_text_is_BusinessRuleError_and_not_saved()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new UpdateQuestionUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateQuestionCommand(question.Id, new string('a', 501), null, 0, "\"1\""), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal(0, questions.UpdateCount);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_is_a_soft_delete()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new DeleteQuestionUseCase(questions, questions);

            var result = await useCase.DeleteAsync(new DeleteQuestionCommand(question.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.False(question.IsActive);
            Assert.Single(questions.Questions);   // the row is still there
            Assert.Equal(1, questions.UpdateCount);
        }

        [Fact]
        public async Task Delete_twice_is_NotFound_the_second_time()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new DeleteQuestionUseCase(questions, questions);

            await useCase.DeleteAsync(new DeleteQuestionCommand(question.Id), Ct);
            var second = await useCase.DeleteAsync(new DeleteQuestionCommand(question.Id), Ct);

            Assert.True(second.HasError<NotFoundError>());
        }
    }
}
