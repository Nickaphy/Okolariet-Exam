using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Question : Aggregateroot
    {
        private readonly List<Answer> answers = new();

        public Guid ModdedTemplateId { get; private set; }
        public string Text { get; private set; } = "";
        public Guid? AssetId { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public IReadOnlyList<Answer> Answers => answers;

        private Question() { }

        public static Question Create(Guid moddedTemplateId, string text, Guid? assetId, int sortOrder)
        {
            Validate(text);
            return new Question { ModdedTemplateId = moddedTemplateId, Text = text, AssetId = assetId, SortOrder = sortOrder };
        }

        public void Update(string text, Guid? assetId, int sortOrder)
        {
            Validate(text);
            Text = text; AssetId = assetId; SortOrder = sortOrder;
        }

        public Answer AddAnswer(string text, bool isCorrect, Guid? assetId, int sortOrder)
        {
            var a = Answer.Create(Id, text, isCorrect, assetId, sortOrder);
            answers.Add(a);
            return a;
        }
        // "Delete" is a soft delete, according to Output.cs
        public void Deactivate() => IsActive = false;

        public bool HasExactlyOneCorrectAnswer() => answers.Count(a => a.IsActive && a.IsCorrect) == 1;

        private static void Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > 500)
                throw new ArgumentException("Question text must be 1-500 characters.", nameof(text));
        }
    }
}