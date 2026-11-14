using Domain.ValueObjects;

namespace Domain.Entities
{
    // The nicknames a user can select after completing a game or quiz.
    public class NicknameWord : Aggregateroot // Get's id from the EntityHierarchy
    {
        public string Text { get; private set; } = "";
        public bool IsActive { get; private set; }

        private NicknameWord() { } // For EF Core

        public NicknameWord(string text, bool isActive = true)
        {
            Text = text;
            IsActive = isActive;
        }

        public static void Validate (string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Nickname text cannot be null or whitespace.", nameof(text));
        }
        
        public static NicknameWord Create(string text, bool isActive = true)
        {
            var nickname = new NicknameWord(text, isActive);
            Validate(nickname.Text);
            return nickname;
        }
    }
}
