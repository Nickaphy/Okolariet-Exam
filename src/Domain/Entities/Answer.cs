using Domain.ValueObjects;

namespace Domain.Entities
{
    // An answer always belongs to a Question. It can only be created through Question.AddAnswer.
    public class Answer : Entity   // Entity, NOT Aggregateroot, because Question is bawzzz
    {
        public Guid QuestionId { get; private set; }
        public string Text { get; private set; } = "";
        public Guid? AssetId { get; private set; }
        public bool IsCorrect { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Answer() { } // EF Core

        // internal = only code inside the Domain project can call this
        internal static Answer Create(Guid questionId, string text, bool isCorrect, Guid? assetId, int sortOrder)
        {
            Validate(text);
            return new Answer
            {
                QuestionId = questionId,
                Text = text,
                IsCorrect = isCorrect,
                AssetId = assetId,
                SortOrder = sortOrder
            };
        }

        public void Update(string text, bool isCorrect, Guid? assetId, int sortOrder)
        {
            Validate(text);
            Text = text; IsCorrect = isCorrect; AssetId = assetId; SortOrder = sortOrder;
        }

        public void Deactivate() => IsActive = false;   // soft delete, just like Question

        private static void Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > 200)   // 200 comes from AnswerInput in Output.cs
                throw new ArgumentException("Answer text must be 1-200 characters.", nameof(text));
        }
    }
}