using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Templates
{
    // All templates staff can build from. They live in code, not in the database.
    public static class TemplateCatalog
    {
        public static readonly IReadOnlyList<Template> All =
        [
            Template.Create("menu-ocean", "Menu (ocean)", TemplateCategory.Menu,
            [
                new SlotDefinition("title", "Titel", SlotKind.Text, Required: true, MaxLength: 120),
                new SlotDefinition("background", "Baggrund", SlotKind.Image),
                new SlotDefinition("button1", "Knap 1", SlotKind.Button, MaxLength: 40),
                new SlotDefinition("button2", "Knap 2", SlotKind.Button, MaxLength: 40),
            ]),

            Template.Create("quiz", "Quiz", TemplateCategory.Quiz,
            [
                new SlotDefinition("title", "Titel", SlotKind.Text, Required: true, MaxLength: 120),
                new SlotDefinition("background", "Baggrund", SlotKind.Image),
            ],
            questionRules: new QuestionRules(maxQuestions: 10, answersPerQuestion: 4)),

            Template.Create("fact", "Faktaside", TemplateCategory.Fact,
            [
                new SlotDefinition("title", "Titel", SlotKind.Text, Required: true, MaxLength: 120),
                new SlotDefinition("body", "Tekst", SlotKind.Text, MaxLength: 500),
                new SlotDefinition("image", "Billede", SlotKind.Image),
                new SlotDefinition("video", "Video", SlotKind.Video),
            ]),
        ];

        public static Template? Find(string type) => All.FirstOrDefault(t => t.Type == type);
    }
}