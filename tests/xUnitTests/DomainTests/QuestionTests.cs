using Domain.Entities;   // Question, Answer

namespace xUnitTests.DomainTests
{
    // Question is the aggregate root, so Answer is tested through it (Answer.Create is internal).
    public class QuestionTests
    {
        [Fact]
        public void Create_sets_properties_and_is_active()
        {
            var moddedTemplateId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var question = Question.Create(moddedTemplateId, "Hvad spiser en sæl?", assetId, 3);

            Assert.Equal(moddedTemplateId, question.ModdedTemplateId);
            Assert.Equal("Hvad spiser en sæl?", question.Text);
            Assert.Equal(assetId, question.AssetId);
            Assert.Equal(3, question.SortOrder);
            Assert.True(question.IsActive);
            Assert.Empty(question.Answers);
        }

        [Fact]
        public void Text_of_500_characters_is_allowed()
        {
            var question = Question.Create(Guid.NewGuid(), new string('a', 500), null, 0);

            Assert.Equal(500, question.Text.Length);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_with_blank_text_throws(string text)
        {
            Assert.Throws<ArgumentException>(() => Question.Create(Guid.NewGuid(), text, null, 0));
        }

        [Fact]
        public void Create_with_501_characters_throws()
        {
            Assert.Throws<ArgumentException>(() => Question.Create(Guid.NewGuid(), new string('a', 501), null, 0));
        }

        [Fact]
        public void Update_changes_text_asset_and_sort_order()
        {
            var question = TestData.Question();
            var assetId = Guid.NewGuid();

            question.Update("Ny tekst", assetId, 7);

            Assert.Equal("Ny tekst", question.Text);
            Assert.Equal(assetId, question.AssetId);
            Assert.Equal(7, question.SortOrder);
        }

        [Fact]
        public void Update_with_invalid_text_throws_and_keeps_old_text()
        {
            var question = TestData.Question();
            var oldText = question.Text;

            Assert.Throws<ArgumentException>(() => question.Update("", null, 0));
            Assert.Equal(oldText, question.Text);
        }

        [Fact]
        public void Deactivate_is_a_soft_delete()
        {
            var question = TestData.Question();

            question.Deactivate();

            Assert.False(question.IsActive);
        }

        [Fact]
        public void AddAnswer_adds_an_active_answer_linked_to_the_question()
        {
            var question = TestData.Question();

            var answer = question.AddAnswer("8", isCorrect: true, assetId: null, sortOrder: 1);

            Assert.Same(answer, Assert.Single(question.Answers));
            Assert.Equal(question.Id, answer.QuestionId);
            Assert.Equal("8", answer.Text);
            Assert.True(answer.IsCorrect);
            Assert.True(answer.IsActive);
        }

        [Fact]
        public void AddAnswer_with_201_characters_throws()
        {
            var question = TestData.Question();

            Assert.Throws<ArgumentException>(() => question.AddAnswer(new string('a', 201), false, null, 0));
            Assert.Empty(question.Answers);
        }

        [Fact]
        public void Answer_Update_changes_values()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);

            answer.Update("8", true, null, 2);

            Assert.Equal("8", answer.Text);
            Assert.True(answer.IsCorrect);
            Assert.Equal(2, answer.SortOrder);
        }

        [Fact]
        public void Answer_Update_with_blank_text_throws()
        {
            var question = TestData.Question();
            var answer = question.AddAnswer("6", false, null, 0);

            Assert.Throws<ArgumentException>(() => answer.Update(" ", false, null, 0));
        }

        [Fact]
        public void HasExactlyOneCorrectAnswer_true_with_one_correct()
        {
            var question = TestData.Question();
            question.AddAnswer("6", false, null, 0);
            question.AddAnswer("8", true, null, 1);

            Assert.True(question.HasExactlyOneCorrectAnswer());
        }

        [Fact]
        public void HasExactlyOneCorrectAnswer_false_with_none_or_two()
        {
            var none = TestData.Question();
            none.AddAnswer("6", false, null, 0);

            var two = TestData.Question();
            two.AddAnswer("6", true, null, 0);
            two.AddAnswer("8", true, null, 1);

            Assert.False(none.HasExactlyOneCorrectAnswer());
            Assert.False(two.HasExactlyOneCorrectAnswer());
        }

        [Fact]
        public void HasExactlyOneCorrectAnswer_ignores_deactivated_answers()
        {
            var question = TestData.Question();
            var oldCorrect = question.AddAnswer("6", true, null, 0);
            question.AddAnswer("8", true, null, 1);

            oldCorrect.Deactivate();

            Assert.True(question.HasExactlyOneCorrectAnswer());
        }
    }
}
