namespace Domain.ValueObjects
{
    // Only quiz templates have these rules.
    public record QuestionRules
    {
        public int MaxQuestions { get; }
        public int AnswersPerQuestion { get; }

        public QuestionRules(int maxQuestions, int answersPerQuestion)
        {
            if (maxQuestions < 1)
                throw new ArgumentException("A quiz must allow at least 1 question.", nameof(maxQuestions));
            if (answersPerQuestion < 2)
                throw new ArgumentException("A question needs at least 2 answers to choose between.", nameof(answersPerQuestion));

            MaxQuestions = maxQuestions;
            AnswersPerQuestion = answersPerQuestion;
        }
    }
}