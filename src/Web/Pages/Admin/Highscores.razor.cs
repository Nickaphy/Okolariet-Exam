using Microsoft.AspNetCore.Components;
using Web.Api;
using Web.Api.Contracts;
using Web.Components;
using Web.Helpers;

namespace Web.Pages.Admin
{
    public partial class Highscores
    {
        // Which dialog is open. Only one at a time; None = no dialog.
        private enum DialogKind { None, ClearBoard, ResetQuestion, DeleteResults }

        // One quiz with its top 10. Players = how many scores are saved in total (not only the top 10).
        private sealed record QuizBoard(ModdedTemplate Quiz, IReadOnlyList<User> Top, int Players);

        // The list of exhibitions (for the drop-down).
        private LoadState projectsState = LoadState.Loading;
        private List<Project> projects = [];
        private string? projectsError;
        private Guid? selectedId;

        // The results of the chosen exhibition.
        private LoadState dataState = LoadState.Loading;
        private string? dataError;
        private ProjectStatistics? stats;
        private List<QuizBoard> boards = [];

        private Notice? notice;
        private DialogKind dialog;
        private ModdedTemplate? activeQuiz;     // ClearBoard: which quiz
        private Question? activeQuestion;       // ResetQuestion: which question

        private Project? SelectedProject => projects.FirstOrDefault(p => p.Id == selectedId);

        protected override Task OnInitializedAsync() => LoadProjectsAsync();

        // ---- loading

        private async Task LoadProjectsAsync()
        {
            projectsState = LoadState.Loading;

            var (all, error) = await ApiCall.GetAll(page => ProjectGateway.ListProjects(page, 100));
            if (error is not null)
            {
                projectsError = error;
                projectsState = LoadState.Failed;
                return;
            }

            projects = all.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            projectsState = LoadState.Loaded;

            // Show the first exhibition straight away, so the page is never empty.
            if (selectedId is null && projects.Count > 0)
            {
                selectedId = projects[0].Id;
                await LoadDataAsync();
            }
        }

        // Everything shown for one exhibition, in three rounds (each round needs the one before):
        //   1. the totals, the template definitions and the list of pages;
        //   2. every quiz page in full (the list has no questions);
        //   3. the top 10 of every quiz.
        private async Task LoadDataAsync()
        {
            if (selectedId is not { } projectId) return;

            dataState = LoadState.Loading;
            dataError = null;

            // 1.
            var statsTask = ApiCall.Get(() => StatisticsGateway.GetProjectStatistics(projectId));
            var templatesTask = ApiCall.Get(() => TemplateGateway.ListTemplates());
            var pagesTask = ApiCall.Get(() => ModdedTemplateGateway.ListModdedTemplates(projectId));

            var (projectStats, statsError) = await statsTask;
            var (templates, templatesError) = await templatesTask;
            var (pages, pagesError) = await pagesTask;
            if ((statsError ?? templatesError ?? pagesError) is { } error1)
            {
                ShowDataError(projectId, error1);
                return;
            }

            // Only quiz pages have highscores. The template definitions say which template types are quizzes.
            var quizTypes = templates!.Where(t => t.Category == TemplateCategory.Quiz).Select(t => t.Type).ToHashSet();
            var quizIds = pages!.Where(p => quizTypes.Contains(p.TemplateType)).OrderBy(p => p.SortOrder).Select(p => p.Id);

            // 2.
            var quizResults = await Task.WhenAll(quizIds.Select(id => ApiCall.Get(() => ModdedTemplateGateway.GetModdedTemplate(id))));
            if (FirstError(quizResults.Select(r => r.Error)) is { } error2)
            {
                ShowDataError(projectId, error2);
                return;
            }
            var quizzes = quizResults.Select(r => r.Value!).ToList();

            // 3.
            var userResults = await Task.WhenAll(quizzes.Select(q => ApiCall.Get(() => UserGateway.ListUsers(q.Id, Sort.BestScore, 1, 10))));
            if (FirstError(userResults.Select(r => r.Error)) is { } error3)
            {
                ShowDataError(projectId, error3);
                return;
            }

            if (selectedId != projectId) return;   // the user picked another exhibition while this was loading

            stats = projectStats;
            boards = quizzes
                .Zip(userResults, (quiz, users) => new QuizBoard(quiz, users.Value!.Items.ToList(), users.Value.TotalCount))
                .ToList();
            dataState = LoadState.Loaded;
        }

        private static string? FirstError(IEnumerable<string?> errors) => errors.FirstOrDefault(e => e is not null);

        private void ShowDataError(Guid projectId, string error)
        {
            if (selectedId != projectId) return;   // an old answer for an exhibition that is no longer chosen
            dataError = error;
            dataState = LoadState.Failed;
        }

        private async Task OnProjectChanged(ChangeEventArgs e)
        {
            if (!Guid.TryParse(e.Value?.ToString(), out var id) || id == selectedId) return;

            selectedId = id;
            notice = null;
            await LoadDataAsync();
        }

        // ---- shown on the page

        private static IEnumerable<Question> QuestionsOf(ModdedTemplate quiz) =>
            quiz.Questions?.OrderBy(q => q.SortOrder) ?? Enumerable.Empty<Question>();

        // "7 af 10 · 70 %"
        private static string ScoreText(User user) =>
            user.Score is null
                ? "–"
                : $"{user.Score.Correct} af {user.Score.TotalQuestions} · {Display.Percent(user.Score.Percentage)}";

        // ---- dialogs

        private void AskClear(ModdedTemplate quiz)
        {
            activeQuiz = quiz;
            dialog = DialogKind.ClearBoard;
        }

        // From QuestionResults: "Nulstil svar" was pressed.
        private void AskReset(Question question)
        {
            activeQuestion = question;
            dialog = DialogKind.ResetQuestion;
        }

        private void AskDeleteResults() => dialog = DialogKind.DeleteResults;

        private void CloseDialog()
        {
            dialog = DialogKind.None;
            activeQuiz = null;
            activeQuestion = null;
        }

        // Handed to ConfirmDialog as Run. ignoreNotFound: already gone is fine.
        private Task<string?> ClearBoardAsync() =>
            ApiCall.Run(() => UserGateway.DeleteQuizUsers(activeQuiz!.Id), ignoreNotFound: true);

        private Task<string?> ResetQuestionAsync() =>
            ApiCall.Run(() => StatisticsGateway.DeleteQuestionResults(activeQuestion!.Id), ignoreNotFound: true);

        private Task<string?> DeleteResultsAsync() =>
            ApiCall.Run(() => StatisticsGateway.DeleteProjectResults(selectedId!.Value), ignoreNotFound: true);

        private Task OnBoardCleared() => Finish("Highscores er ryddet.");

        private Task OnQuestionReset() => Finish("Svarene til spørgsmålet er nulstillet.");

        private Task OnResultsDeleted() => Finish("Alle resultater for udstillingen er slettet.");

        // Closes the dialog, says what happened and reloads the numbers.
        private async Task Finish(string text)
        {
            CloseDialog();
            notice = Notice.Success(text);
            await LoadDataAsync();
        }
    }
}
