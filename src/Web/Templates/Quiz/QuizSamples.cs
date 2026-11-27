namespace Web.Templates.Quiz
{
    // Example quizzes for the template gallery and the template picker. The right answers are included,
    // so the gallery can play the whole quiz in demo mode without a server (see QuizRunner.Demo).
    public static class QuizSamples
    {
        // A page of the given quiz type with a title, an intro and the example questions.
        public static PageContent Page(string templateType, IReadOnlyList<QuizQuestion> questions) =>
            PageContent.Sample(templateType,
                ("title", "Sortér dit affald"),
                ("intro", "Ved du, hvor tingene skal hen? Svar på spørgsmålene og få point."),
                ("factTitle", "Vidste du?"),
                ("factText", "En plastflaske kan ligge 450 år i naturen, før den er væk. Når du sorterer den, kan den blive til en ny flaske."))
            with { Questions = questions };

        // Four answers per question. The text marked with * is the right one.
        public static readonly IReadOnlyList<QuizQuestion> FourAnswers =
        [
            Question("Hvor skal et tomt syltetøjsglas hen?", "Glas*", "Metal", "Restaffald", "Papir"),
            Question("Hvor lang tid tager det for en plastflaske at forsvinde i naturen?", "1 år", "10 år", "100 år", "450 år*"),
            Question("Hvad kan en gammel avis blive til?", "Nyt papir*", "Glas", "Metal", "Ingenting"),
        ];

        // Two answers per question, for the true/false quiz.
        public static readonly IReadOnlyList<QuizQuestion> TrueFalse =
        [
            Question("En mælkekarton skal i papir.", "Sandt", "Falsk*"),
            Question("Batterier må ikke komme i skraldespanden.", "Sandt*", "Falsk"),
            Question("Madrester kan blive til biogas.", "Sandt*", "Falsk"),
        ];

        private static QuizQuestion Question(string text, params string[] answers) => new(
            Guid.NewGuid(), text, null,
            answers.Select(a => new QuizAnswer(Guid.NewGuid(), a.TrimEnd('*'), null, IsCorrect: a.EndsWith('*'))).ToList());
    }
}
