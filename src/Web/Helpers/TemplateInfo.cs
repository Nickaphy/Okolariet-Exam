using Web.Api.Contracts;
using Web.Services.Editor;

namespace Web.Helpers
{
    // This helper exists to translate some of the UI "Items" or "Assets" so we don't have to repeat ourselves.
    // Danish texts about the fixed templates. Presentation only.
    public static class TemplateInfo
    {
        public static string CategoryName(TemplateCategory category) => category switch //TO DO: Make sure to add Game when that type is ready
        {
            TemplateCategory.Quiz => "Quiz",
            TemplateCategory.Fact => "Fakta",
            _ => "Menu"
        };

        // Hand-written text for a template that deserves a nicer description. Key = template type.
        // A template that is not listed here still gets a description, made from its slots.
        private static readonly Dictionary<string, string> Overrides = new();

        public static string Describe(Template template)
        {
            if (Overrides.TryGetValue(template.Type, out var text)) return text;

            if (template.QuestionRules is { } rules)
                return $"Op til {rules.MaxQuestions} spørgsmål med {rules.AnswersPerQuestion} svarmuligheder.";

            var slots = template.Slots.Where(s => s.Key != PathConvention.NextSlotKey).ToList();
            var parts = new List<string>(); 
            Add(parts, slots, SlotDefinitionKind.Button, "knap", "knapper"); //Ideally should have used a type of localizer, has added technical debt. But it prevents us having to state Button is knap over and over.
            Add(parts, slots, SlotDefinitionKind.Text, "tekstfelt", "tekstfelter");
            Add(parts, slots, SlotDefinitionKind.Image, "billede", "billeder");
            Add(parts, slots, SlotDefinitionKind.Video, "video", "videoer");
            Add(parts, slots, SlotDefinitionKind.Audio, "lydfil", "lydfiler");

            return parts.Count == 0 ? string.Empty : string.Join(", ", parts) + ".";
        }

        private static void Add(List<string> parts, List<SlotDefinition> slots, SlotDefinitionKind kind, string one, string many) //Translates words in as flexible a way as possible. Can both handle one or multiple
        {
            var count = slots.Count(s => s.Kind == kind);
            if (count > 0) parts.Add($"{count} {(count == 1 ? one : many)}");
        }
    }
}