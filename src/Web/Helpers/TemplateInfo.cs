using Web.Api.Contracts;
using Web.Services.Editor;

namespace Web.Helpers
{
    // Danish texts about the templates, for the template picker. Presentation only.
    public static class TemplateInfo
    {
        // The group headings in the picker. TODO: add Game when that category exists.
        public static string CategoryName(TemplateCategory category) => category switch
        {
            TemplateCategory.Quiz => "Quiz",
            TemplateCategory.Fact => "Fakta",
            _ => "Menu"
        };

        // A hand-written description for a template. Key = template type.
        // A template that is not listed here still gets one, counted from its slots ("4 knapper, 2 tekstfelter.").
        private static readonly Dictionary<string, string> Overrides = new()
        {
            ["fact-image-text"] = "Overskrift og tekst ved siden af et billede. Kan læses højt.",
            ["fact-hero"] = "Ét stort billede med en tekstboks nederst.",
            ["fact-stats"] = "Op til tre kort med et stort tal og en kort forklaring.",
            ["fact-steps"] = "Op til fire trin efter hinanden, hver med billede og tekst.",
            ["fact-two-images"] = "To billeder side om side, fx før og efter.",
            ["fact-list"] = "Op til tre korte punkter ved siden af et billede.",
            ["fact-video"] = "En stor video med en tekst under.",
            ["fact-video-only"] = "Kun en video, så stor som skærmen tillader.",

            ["quiz-classic"] = "Spørgsmål med et billede og fire svar. Op til 10 spørgsmål.",
            ["quiz-factbox"] = "Spørgsmål og svar til venstre, en faktaboks med tekst eller video til højre.",
            ["quiz-image-answers"] = "Svarene er billeder. Godt til de mindste.",
            ["quiz-video"] = "En kort video til hvert spørgsmål, derefter fire svar.",
            ["quiz-true-false"] = "Et udsagn og to store knapper, fx sandt eller falsk.",
        };

        public static string Describe(Template template)
        {
            if (Overrides.TryGetValue(template.Type, out var text)) return text;

            if (template.QuestionRules is { } rules)
                return $"Op til {rules.MaxQuestions} spørgsmål med {rules.AnswersPerQuestion} svarmuligheder.";

            var slots = template.Slots.Where(s => s.Key != PathConvention.NextSlotKey).ToList();
            var parts = new List<string>(); 
            Add(parts, slots, SlotDefinitionKind.Button, "knap", "knapper");
            Add(parts, slots, SlotDefinitionKind.Text, "tekstfelt", "tekstfelter");
            Add(parts, slots, SlotDefinitionKind.Image, "billede", "billeder");
            Add(parts, slots, SlotDefinitionKind.Video, "video", "videoer");
            Add(parts, slots, SlotDefinitionKind.Audio, "lydfil", "lydfiler");

            return parts.Count == 0 ? string.Empty : string.Join(", ", parts) + ".";
        }

        // Adds e.g. "1 knap" or "3 knapper" when the template has slots of that kind.
        private static void Add(List<string> parts, List<SlotDefinition> slots, SlotDefinitionKind kind, string one, string many)
        {
            var count = slots.Count(s => s.Kind == kind);
            if (count > 0) parts.Add($"{count} {(count == 1 ? one : many)}");
        }
    }
}