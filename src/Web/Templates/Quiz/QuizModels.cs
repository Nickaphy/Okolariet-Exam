using Microsoft.AspNetCore.Components;
using Web.Api.Contracts;

namespace Web.Templates.Quiz
{
    // One quiz question as the quiz templates see it (made from the published snapshot or the editor's data, see PageContent).
    //   AssetId = the question's picture or video (which of the two depends on the template).
    public sealed record QuizQuestion(Guid Id, string Text, Guid? AssetId, IReadOnlyList<QuizAnswer> Answers);

    // One answer option.
    //   IsCorrect is only known in the editor and in the gallery's examples. In the kiosk it is always null:
    //   the published snapshot never contains the right answers, the server says it after the guest has answered.
    public sealed record QuizAnswer(Guid Id, string Text, Guid? AssetId, bool? IsCorrect);

    // What the parts of a question screen (QuizQuestionText, QuizMedia, AnswerTiles) need to know right now.
    // QuizRunner makes a new one every time something changes and hands it down as a cascading value.
    public sealed record QuizTurn(
        QuizQuestion Question,
        int Number,                   // 1-based: "Spørgsmål 3 / 10"
        int Total,
        Guid? ChosenId,               // the answer the guest tapped (null = not answered yet)
        Guid? CorrectId,              // the right answer, once known
        bool Locked,                  // true = tapping does nothing (already answered, sending, or a preview)
        EventCallback<Guid> Choose)   // tell QuizRunner which answer was tapped
    {
        public bool Answered => CorrectId is not null;
    }

    // How each answer looks: By position (1st ▲, 2nd ●, 3rd ■, 4th ◆).
    // Shapes and not only colours, so the answers can also be told apart by people who are colour-blind.
    public static class AnswerShapes
    {
        private static readonly string[] Shapes = ["▲", "●", "■", "◆"];

        public static string Of(int index) => Shapes[index % Shapes.Length];
    }

    // The guest's highscore name: two approved words and a number (0-99). See NicknamePicker.
    public sealed record NicknameChoice(NicknameWord First, NicknameWord Second, int Number);
}
