using Domain.Entities;       // Template, Asset, ModdedTemplate, Question
using Domain.Enums;          // SlotKind, AssetKind, TemplateCategory
using Domain.ValueObjects;   // SlotDefinition, QuestionRules

namespace xUnitTests
{
    // Ready-made domain objects, so each test only has to write the part it cares about.
    public static class TestData
    {
        public const string MenuType = "menu-test";
        public const string QuizType = "quiz-test";

        // A menu template with one slot of every kind.
        public static Template MenuTemplate() => Template.Create(
            MenuType,
            "Test menu",
            TemplateCategory.Menu,
            [
                new SlotDefinition("heading", "Overskrift", SlotKind.Text, Required: true, MaxLength: 40),
                new SlotDefinition("background", "Baggrund", SlotKind.Image),
                new SlotDefinition("video", "Video", SlotKind.Video),
                new SlotDefinition("sound", "Lyd", SlotKind.Audio),
                new SlotDefinition("button1", "Knap 1", SlotKind.Button, MaxLength: 30)
            ]);

        // A quiz template: has QuestionRules, so it supports questions.
        public static Template QuizTemplate() => Template.Create(
            QuizType,
            "Test quiz",
            TemplateCategory.Quiz,
            [new SlotDefinition("heading", "Overskrift", SlotKind.Text, MaxLength: 40)],
            questionRules: new QuestionRules(maxQuestions: 10, answersPerQuestion: 4));

        public static Asset Asset(AssetKind kind = AssetKind.Image) => kind switch
        {
            AssetKind.Image => Domain.Entities.Asset.Create(kind, "cat.png", "stored-cat.png", "image/png", 1_000, DateTimeOffset.UnixEpoch),
            AssetKind.Video => Domain.Entities.Asset.Create(kind, "clip.mp4", "stored-clip.mp4", "video/mp4", 1_000, DateTimeOffset.UnixEpoch),
            _ => Domain.Entities.Asset.Create(kind, "song.mp3", "stored-song.mp3", "audio/mpeg", 1_000, DateTimeOffset.UnixEpoch)
        };

        public static ModdedTemplate MenuModdedTemplate(Guid? projectId = null, int sortOrder = 0)
            => ModdedTemplate.Create(projectId ?? Guid.NewGuid(), MenuTemplate(), "Forside", null, sortOrder);

        public static Question Question(Guid? moddedTemplateId = null, int sortOrder = 0)
            => Domain.Entities.Question.Create(moddedTemplateId ?? Guid.NewGuid(), "Hvor mange ben har en edderkop?", null, sortOrder);
    }
}
