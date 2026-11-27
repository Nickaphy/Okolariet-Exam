using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Ports.Sessions;

namespace Web.Templates.Quiz
{
    // One answered question, kept for the result screen ("Dine svar").
    public sealed record AnsweredQuestion(QuizQuestion Question, Guid ChosenId, Guid CorrectId)
    {
        public bool IsCorrect => ChosenId == CorrectId;
    }

    public partial class QuizRunner
    {
        private enum Phase { Start, Question, Finished }

        // Points are only for fun on screen (Kahoot style). The saved highscore is the server's own count of right answers.
        public const int PointsPerAnswer = 10;

        [CascadingParameter] public TemplateScope Scope { get; set; } = default!;

        // The template's question screen (built from QuizQuestionText, QuizMedia and AnswerTiles).
        [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;

        [Inject] private ISessionGateway SessionGateway { get; set; } = default!;

        private Phase phase = Phase.Start;
        private int index;                          // the question on screen (0-based)
        private Guid? chosenId;                     // what the guest tapped on this question
        private Guid? correctId;                    // the right answer, once known (shows the feedback card)
        private bool lastWasCorrect;
        private bool busy;                          // waiting for the server
        private string? error;
        private Guid? sessionId;                    // the guest's session on the server (live mode only)
        private readonly List<AnsweredQuestion> history = [];

        private IReadOnlyList<QuizQuestion> Questions => Scope.Page.Questions;
        private QuizQuestion Current => Questions[index];
        private bool IsLastQuestion => index == Questions.Count - 1;
        private int Points => history.Count(h => h.IsCorrect) * PointsPerAnswer;

        // Two ways to play:
        //   Demo: the right answers are in the data (editor, gallery examples). We judge ourselves, nothing is saved.
        //   Live: the kiosk. The server judges every answer and keeps the guest's session.
        private bool Demo => Questions.Any(q => q.Answers.Any(a => a.IsCorrect is not null));

        private string QuestionCountText => Questions.Count switch
        {
            0 => "Quizzen har ingen spørgsmål endnu.",
            1 => "1 spørgsmål",
            var n => $"{n} spørgsmål"
        };

        private string CorrectAnswerText => Current.Answers.FirstOrDefault(a => a.Id == correctId)?.Text ?? "";

        // What the question screen's parts need (see QuizTurn). A new one each render, so they always see the latest state.
        private QuizTurn Turn => new(
            Current, index + 1, Questions.Count, chosenId, correctId,
            Locked: Scope.Preview || busy || correctId is not null,
            Choose: EventCallback.Factory.Create<Guid>(this, ChooseAsync));

        protected override void OnParametersSet()
        {
            // In a preview (the editor) the first question is shown straight away, and nothing can be tapped.
            if (Scope.Preview && phase == Phase.Start && Questions.Count > 0) phase = Phase.Question;
        }

        // "Start quiz". Live: first get a session from the server; every answer is recorded against it.
        private async Task StartAsync()
        {
            error = null;
            if (!Demo)
            {
                if (Scope.ProjectId is not Guid projectId)
                {
                    error = "Quizzen kan kun spilles på skærmen i udstillingen.";
                    return;
                }

                busy = true;
                var (session, sessionError) = await ApiCall.Get(() => SessionGateway.CreateSession(projectId));
                busy = false;
                if (sessionError is not null)
                {
                    error = sessionError;
                    return;
                }
                sessionId = session!.Id;
            }

            history.Clear();
            index = 0;
            ResetQuestion();
            phase = Phase.Question;
        }

        // The guest tapped an answer. Find out if it was right, then the feedback card shows (because correctId is set).
        private async Task ChooseAsync(Guid answerId)
        {
            if (chosenId is not null || busy || Scope.Preview) return;

            chosenId = answerId;
            error = null;

            if (Demo)
            {
                correctId = Current.Answers.FirstOrDefault(a => a.IsCorrect == true)?.Id ?? answerId;
            }
            else
            {
                busy = true;
                var (result, resultError) = await ApiCall.Get(() => SessionGateway.SubmitResult(sessionId!.Value, Current.Id, answerId));
                busy = false;
                if (resultError is not null)
                {
                    error = resultError + " Tryk på et svar igen.";
                    chosenId = null;   // let the guest try again
                    return;
                }
                correctId = result!.CorrectAnswerId;
            }

            lastWasCorrect = answerId == correctId;
            history.Add(new AnsweredQuestion(Current, answerId, correctId!.Value));
        }

        // "Næste spørgsmål" on the feedback card.
        private void Next()
        {
            if (IsLastQuestion)
            {
                phase = Phase.Finished;
                return;
            }
            index++;
            ResetQuestion();
        }

        // "Spil igen" on the result screen: back to the start (a new game gets a new session).
        private void Restart()
        {
            sessionId = null;
            history.Clear();
            phase = Phase.Start;
        }

        private void ResetQuestion()
        {
            chosenId = null;
            correctId = null;
            error = null;
        }
    }
}
