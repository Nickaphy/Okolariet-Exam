using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Templates
{
    // All templates staff can build from.
    public static class TemplateCatalog
    {
        public static readonly IReadOnlyList<Template> All =
        [
            // Menus are named after their layout, not a theme: colours and font come from the Appearance.
            // The Blazor components are in Web/Templates/Menus; the slot keys (button1..N) must match them.
            Menu("menu-buttons", "Hovedmenu – 4 knapper i række", buttons: 4),
            Menu("menu-split-4", "Hovedmenu – 4 knapper ved siden af", buttons: 4),
            Menu("menu-three",   "Hovedmenu – 3 store knapper", buttons: 3),
            Menu("menu-six",     "Hovedmenu – 6 knapper", buttons: 6, buttonMaxLength: 24),
            Menu("menu-eight",   "Hovedmenu – 8 knapper", buttons: 8, buttonMaxLength: 20),

            // ---- Quizzes (Web/Templates/Quiz). They share the game (QuizRunner); the layout of a question differs.
            // The questions and answers are the quiz's own (Questions/Answers), the slots are the start screen.
            // The "next" slot (the page after the quiz) is added by Template.Create.
            Quiz("quiz-classic", "Quiz – spørgsmål og 4 svar"),
            Quiz("quiz-factbox", "Quiz – med faktaboks",
            [
                Text("factTitle", "Faktaboks: overskrift", 40),
                Text("factText", "Faktaboks: tekst", 300),
                Image("factImage", "Faktaboks: billede"),
                new SlotDefinition("factVideo", "Faktaboks: video", SlotKind.Video),
            ]),
            Quiz("quiz-image-answers", "Quiz – svar med billeder"),
            Quiz("quiz-video", "Quiz – video og spørgsmål"),
            Quiz("quiz-true-false", "Quiz – sandt eller falsk", answersPerQuestion: 2),

            // ---- Fact pages (Web/Templates/Facts). Every field is optional.
            // The "next" slot (the page after this one) is added by Template.Create.
            Fact("fact-image-text", "Fakta – billede og tekst",
            [
                Title(), Text("body", "Tekst", 400), Image("image", "Billede"), Audio(),
            ]),

            Fact("fact-hero", "Fakta – stort billede",
            [
                Title(), Text("body", "Tekst", 240), Image("image", "Stort billede"), Audio(),
            ]),

            Fact("fact-stats", "Fakta – tre tal",
            [
                Title(), Audio(),
                .. Enumerable.Range(1, 3).SelectMany(n => new[]
                {
                    Image($"stat{n}Image", $"Ikon {n}"),
                    Text($"stat{n}", $"Tal {n}", 12),
                    Text($"stat{n}Text", $"Forklaring {n}", 80),
                }),
            ]),

            Fact("fact-steps", "Fakta – trin for trin",
            [
                Title(), Audio(),
                .. Enumerable.Range(1, 4).SelectMany(n => new[]
                {
                    Image($"step{n}Image", $"Billede, trin {n}"),
                    Text($"step{n}", $"Overskrift, trin {n}", 30),
                    Text($"step{n}Text", $"Tekst, trin {n}", 90),
                }),
            ]),

            Fact("fact-two-images", "Fakta – to billeder",
            [
                Title(), Audio(),
                Image("image1", "Billede 1"), Text("caption1", "Billedtekst 1", 80),
                Image("image2", "Billede 2"), Text("caption2", "Billedtekst 2", 80),
            ]),

            Fact("fact-list", "Fakta – billede og punkter",
            [
                Title(), Audio(), Image("image", "Billede"),
                .. Enumerable.Range(1, 3).Select(n => Text($"point{n}", $"Punkt {n}", 100)),
            ]),

            Fact("fact-video", "Fakta – video med tekst",
            [
                new SlotDefinition("video", "Video", SlotKind.Video), Title(),
            ]),

            Fact("fact-video-only", "Fakta – kun video",
            [
                new SlotDefinition("video", "Video", SlotKind.Video),
            ]),

        ];

        public static Template? Find(string type) => All.FirstOrDefault(t => t.Type == type);

        // A main menu: heading, intro, background image and x buttons. Nothing is required.
        private static Template Menu(string type, string displayName, int buttons, int buttonMaxLength = 30) =>
            Template.Create(type, displayName, TemplateCategory.Menu,
            [
                new SlotDefinition("title", "Overskrift", SlotKind.Text, MaxLength: 60),
                new SlotDefinition("intro", "Undertekst", SlotKind.Text, MaxLength: 160),
                new SlotDefinition("background", "Baggrundsbillede", SlotKind.Image),
                .. Enumerable.Range(1, buttons).Select(i =>
                    new SlotDefinition($"button{i}", $"Knap {i}", SlotKind.Button, MaxLength: buttonMaxLength)),
            ]);

        // A quiz: a start screen (title, intro, background) plus any extra slots, and up to 10 questions.
        // Nothing is required, like the other templates.
        private static Template Quiz(string type, string displayName, IReadOnlyList<SlotDefinition>? extraSlots = null,
            int answersPerQuestion = 4) =>
            Template.Create(type, displayName, TemplateCategory.Quiz,
            [
                Title(80),
                Text("intro", "Undertekst", 160),
                Image("background", "Baggrundsbillede"),
                .. extraSlots ?? [],
            ],
            questionRules: new QuestionRules(maxQuestions: 10, answersPerQuestion: answersPerQuestion));

        // Creates the different fact templates. 4 different types: Title, Text, Image and Audio
       private static Template Fact(string type, string displayName, IReadOnlyList<SlotDefinition> slots) =>
        Template.Create(type, displayName, TemplateCategory.Fact, slots);

        private static SlotDefinition Title(int max = 100) => new("title", "Overskrift", SlotKind.Text, MaxLength: max);
        private static SlotDefinition Text(string key, string label, int max) => new(key, label, SlotKind.Text, MaxLength: max);
        private static SlotDefinition Image(string key, string label) => new(key, label, SlotKind.Image);
        private static SlotDefinition Audio() => new("audio", "Oplæsning (lydfil)", SlotKind.Audio);
    }
}