using Domain.Entities;       // Template
using Domain.Enums;          // TemplateCategory, SlotKind
using Domain.ValueObjects;   // SlotDefinition, QuestionRules

namespace xUnitTests.DomainTests
{
    public class TemplateTests
    {
        [Fact]
        public void Create_sets_properties_and_defaults_appearance_fields_to_empty()
        {
            var template = TestData.MenuTemplate();

            Assert.Equal(TestData.MenuType, template.Type);
            Assert.Equal(TemplateCategory.Menu, template.Category);
            Assert.Equal(5, template.Slots.Count);
            Assert.Empty(template.AppearanceFields);
            Assert.Null(template.QuestionRules);
        }

        [Theory]
        [InlineData("", "Menu")]
        [InlineData("menu", "")]
        [InlineData("  ", "Menu")]
        public void Create_without_type_or_display_name_throws(string type, string displayName)
        {
            Assert.Throws<ArgumentException>(() => Template.Create(type, displayName, TemplateCategory.Menu, []));
        }

        [Fact]
        public void Create_with_duplicate_slot_keys_throws()
        {
            SlotDefinition[] slots =
            [
                new("button1", "Knap 1", SlotKind.Button),
                new("button1", "Knap 1 igen", SlotKind.Button)
            ];

            Assert.Throws<ArgumentException>(() => Template.Create("menu", "Menu", TemplateCategory.Menu, slots));
        }

        [Fact]
        public void Quiz_without_question_rules_throws()
        {
            Assert.Throws<ArgumentException>(() => Template.Create("quiz", "Quiz", TemplateCategory.Quiz, []));
        }

        [Fact]
        public void FindSlot_returns_slot_or_null()
        {
            var template = TestData.MenuTemplate();

            Assert.Equal(SlotKind.Button, template.FindSlot("button1")?.Kind);
            Assert.Null(template.FindSlot("button99"));
            Assert.Null(template.FindSlot("Button1"));   // keys are case-sensitive
        }

        [Fact]
        public void SupportsQuestions_only_with_question_rules()
        {
            Assert.True(TestData.QuizTemplate().SupportsQuestions());
            Assert.False(TestData.MenuTemplate().SupportsQuestions());
        }

        [Theory]
        [InlineData(0, 4)]   // must allow at least 1 question
        [InlineData(5, 1)]   // must have at least 2 answers to choose between
        public void QuestionRules_with_invalid_numbers_throws(int maxQuestions, int answersPerQuestion)
        {
            Assert.Throws<ArgumentException>(() => new QuestionRules(maxQuestions, answersPerQuestion));
        }

        [Fact]
        public void QuestionRules_with_valid_numbers_is_kept()
        {
            var rules = new QuestionRules(10, 4);

            Assert.Equal(10, rules.MaxQuestions);
            Assert.Equal(4, rules.AnswersPerQuestion);
        }
    }
}
