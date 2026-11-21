using Domain.Enums;          // TemplateCategory
using Domain.ValueObjects;   // SlotDefinition, AppearanceFieldDefinition, QuestionRules

namespace Domain.Entities
{
    // A template is a fixed blueprint (menu, quiz, fact page) that staff build ModdedTemplates from.

    public class Template
    {
        public string Type { get; }
        public string DisplayName { get; }
        public TemplateCategory Category { get; }
        public IReadOnlyList<SlotDefinition> Slots { get; }
        public IReadOnlyList<AppearanceFieldDefinition> AppearanceFields { get; }
        public QuestionRules? QuestionRules { get; }

        private Template(string type, string displayName, TemplateCategory category,
                         IReadOnlyList<SlotDefinition> slots,
                         IReadOnlyList<AppearanceFieldDefinition> appearanceFields,
                         QuestionRules? questionRules)
        {
            Type = type;
            DisplayName = displayName;
            Category = category;
            Slots = slots;
            AppearanceFields = appearanceFields;
            QuestionRules = questionRules;
        }

        public static Template Create(string type, string displayName, 
        TemplateCategory category, 
        IReadOnlyList<SlotDefinition> slots,
        IReadOnlyList<AppearanceFieldDefinition>? appearanceFields = null,
        QuestionRules? questionRules = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Template type is required.", nameof(type));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(displayName));
            if (slots.Select(s => s.Key).Distinct().Count() != slots.Count)
                throw new ArgumentException("Slot keys must be unique within a template.", nameof(slots));
            if (category == TemplateCategory.Quiz && questionRules is null)
                throw new ArgumentException("A quiz template must have question rules.", nameof(questionRules));

            return new Template(type, displayName, category, slots, appearanceFields ?? [], questionRules);
        }

        // find a slot by its key, or null if it doesn't exist.
        public SlotDefinition? FindSlot(string key) => Slots.FirstOrDefault(s => s.Key == key);

        // only templates with question rules can hold questions.
        public bool SupportsQuestions() => QuestionRules is not null;
    }
}