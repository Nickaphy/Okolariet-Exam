using Application.Commands.AnswerCommands;   // CreateAnswerCommand, UpdateAnswerCommand, DeleteAnswerCommand
using Application.ErrorMessages;             // NotFoundError, BusinessRuleError
using Application.UseCases.Answers;          // the answer use cases
using xUnitTests.ApplicationTests.Fakes;     // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class AnswerUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeQuestionRepository questions = new();
        private readonly FakeAssetRepository assets = new();

        // ---------- Create ----------

        [Fact]
        public async Task Create_adds_answer_to_question_and_saves_the_question()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(question.Id, "8", null, true, 0), Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(result.Value, Assert.Single(question.Answers));
            Assert.Equal(1, questions.UpdateCount);
        }

        [Fact]
        public async Task Create_on_unknown_question_is_NotFound()
        {
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(Guid.NewGuid(), "8", null, true, 0), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Create_on_deactivated_question_is_NotFound()
        {
            var question = TestData.Question();
            question.Deactivate();
            questions.Questions.Add(question);
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(question.Id, "8", null, true, 0), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Create_with_unknown_asset_is_NotFound()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(question.Id, "8", Guid.NewGuid(), true, 0), Ct);

            Assert.True(result.HasError<NotFoundError>());
            Assert.Empty(question.Answers);
        }

        [Fact]
        public async Task Create_with_existing_asset_works()
        {
            var question = TestData.Question();
            var asset = TestData.Asset();
            questions.Questions.Add(question);
            assets.Assets.Add(asset);
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(question.Id, "8", asset.Id, true, 0), Ct);

            Assert.Equal(asset.Id, result.Value.AssetId);
        }

        [Fact]
        public async Task Create_with_too_long_text_is_BusinessRuleError()
        {
            var question = TestData.Question();
            questions.Questions.Add(question);
            var useCase = new CreateAnswerUseCase(questions, assets, questions);

            var result = await useCase.CreateAsync(new CreateAnswerCommand(question.Id, new string('a', 201), null, true, 0), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal(0, questions.UpdateCount);
        }

        // ---------- Update ----------

        [Fact]
        public async Task Update_changes_answer_and_saves_question()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);
            questions.Questions.Add(question);
            var useCase = new UpdateAnswerUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateAnswerCommand(answer.Id, "8", null, true, 1), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("8", answer.Text);
            Assert.True(answer.IsCorrect);
            Assert.Equal(1, questions.UpdateCount);
        }

        [Fact]
        public async Task Update_unknown_answer_is_NotFound()
        {
            var useCase = new UpdateAnswerUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateAnswerCommand(Guid.NewGuid(), "8", null, true, 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Update_deactivated_answer_is_NotFound()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);
            answer.Deactivate();
            questions.Questions.Add(question);
            var useCase = new UpdateAnswerUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateAnswerCommand(answer.Id, "8", null, true, 1), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Update_with_blank_text_is_BusinessRuleError()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);
            questions.Questions.Add(question);
            var useCase = new UpdateAnswerUseCase(questions, assets, questions);

            var result = await useCase.UpdateAsync(new UpdateAnswerCommand(answer.Id, "", null, true, 1), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal("6", answer.Text);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_is_a_soft_delete()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);
            questions.Questions.Add(question);
            var useCase = new DeleteAnswerUseCase(questions, questions);

            var result = await useCase.DeleteAsync(new DeleteAnswerCommand(answer.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.False(answer.IsActive);
            Assert.Single(question.Answers);   // still there, just inactive
            Assert.Equal(1, questions.UpdateCount);
        }

        [Fact]
        public async Task Delete_twice_is_NotFound_the_second_time()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);
            questions.Questions.Add(question);
            var useCase = new DeleteAnswerUseCase(questions, questions);

            await useCase.DeleteAsync(new DeleteAnswerCommand(answer.Id), Ct);
            var second = await useCase.DeleteAsync(new DeleteAnswerCommand(answer.Id), Ct);

            Assert.True(second.HasError<NotFoundError>());
        }
    }
}
