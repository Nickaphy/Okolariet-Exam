using Microsoft.AspNetCore.Components;
using Refit;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Pages.Admin
{
    public partial class Highscores
    {
        private enum LoadState { Loading, Loaded, Failed }

        private enum DialogKind { None, ClearBoard, ResetQuestion, DeleteResults }

        private sealed record Notice(string Kind, string Text);

        // One quiz with its top scores. Players is how many scores are saved in total (not only the top 10).
        private sealed record QuizBoard(ModdedTemplate Quiz, IReadOnlyList<User> Top, int Players);

        private LoadState projectsState = LoadState.Loading;
        private List<Project> projects = [];
        private string? projectsError;
        private Guid? selectedId;

        private LoadState dataState = LoadState.Loading;
        private string? dataError;
        private ProjectStatistics? stats;
        private List<QuizBoard> boards = [];

        // Answer statistics are fetched per question the first time it is opened.
        private readonly HashSet<Guid> expanded = [];
        private readonly HashSet<Guid> loadingQuestions = [];
        private readonly Dictionary<Guid, QuestionStatistics> questionStats = [];
        private readonly Dictionary<Guid, string> questionErrors = [];

        private Notice? notice;
        private DialogKind dialog;
        private ModdedTemplate? activeQuiz;
        private Question? activeQuestion;

        private Project? SelectedProject => projects.FirstOrDefault(p => p.Id == selectedId);

        protected override Task OnInitializedAsync() => LoadProjectsAsync();

        // ---- loading

        private async Task LoadProjectsAsync()
        {
            projectsState = LoadState.Loading;
            try
            {
                // Paged (max 100 per page); a museum has few projects, so fetch them all.
                var all = new List<Project>();
                for (var page = 1; ; page++)
                {
                    var response = await ProjectsApi.ListProjects(page, 100);
                    if (!response.IsSuccessStatusCode || response.Content is null)
                    {
                        projectsError = ApiErrors.Describe(response);
                        projectsState = LoadState.Failed;
                        return;
                    }

                    all.AddRange(response.Content.Items);
                    if (response.Content.Items.Count == 0 || all.Count >= response.Content.TotalCount) break;
                }

                projects = all.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
                projectsState = LoadState.Loaded;
            }
            catch (HttpRequestException)
            {
                projectsError = ApiErrors.Offline;
                projectsState = LoadState.Failed;
                return;
            }

            // Show the first exhibition straight away, so the page is never empty.
            if (selectedId is null && projects.Count > 0)
            {
                selectedId = projects[0].Id;
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            if (selectedId is not { } projectId) return;

            dataState = LoadState.Loading;
            dataError = null;
            expanded.Clear();
            loadingQuestions.Clear();
            questionStats.Clear();
            questionErrors.Clear();

            try
            {
                var statsTask = StatisticsApi.GetProjectStatistics(projectId);
                var templatesTask = TemplatesApi.ListTemplates();
                var pagesTask = ModdedTemplatesApi.ListModdedTemplates(projectId);
                await Task.WhenAll(statsTask, templatesTask, pagesTask);

                var statsResponse = await statsTask;
                var templatesResponse = await templatesTask;
                var pagesResponse = await pagesTask;
                if (FirstFailure(statsResponse, templatesResponse, pagesResponse) is { } failure)
                {
                    SetDataFailed(projectId, failure);
                    return;
                }

                // Only quiz pages have highscores. The template list says which types are quizzes.
                var quizTypes = templatesResponse.Content!
                    .Where(t => t.Category == TemplateCategory.Quiz)
                    .Select(t => t.Type)
                    .ToHashSet();
                var quizSummaries = pagesResponse.Content!
                    .Where(p => quizTypes.Contains(p.TemplateType))
                    .OrderBy(p => p.SortOrder)
                    .ToList();

                // The summaries have no questions, so each quiz is fetched in full.
                var quizResponses = await Task.WhenAll(
                    quizSummaries.Select(s => ModdedTemplatesApi.GetModdedTemplate(s.Id)));
                if (FirstFailure(quizResponses) is { } quizFailure)
                {
                    SetDataFailed(projectId, quizFailure);
                    return;
                }

                var quizzes = quizResponses.Select(r => r.Content!).ToList();
                var userResponses = await Task.WhenAll(
                    quizzes.Select(q => UsersApi.ListUsers(q.Id, Sort.BestScore, 1, 10)));
                if (FirstFailure(userResponses) is { } userFailure)
                {
                    SetDataFailed(projectId, userFailure);
                    return;
                }

                if (selectedId != projectId) return; // the user picked another exhibition meanwhile

                stats = statsResponse.Content;
                boards = quizzes
                    .Zip(userResponses, (quiz, users) =>
                        new QuizBoard(quiz, users.Content!.Items.ToList(), users.Content.TotalCount))
                    .ToList();
                dataState = LoadState.Loaded;
            }
            catch (HttpRequestException)
            {
                if (selectedId != projectId) return;
                dataError = ApiErrors.Offline;
                dataState = LoadState.Failed;
            }
        }

        private static IApiResponse? FirstFailure(params IApiResponse[] responses) =>
            responses.FirstOrDefault(r => !r.IsSuccessStatusCode);

        private void SetDataFailed(Guid projectId, IApiResponse response)
        {
            if (selectedId != projectId) return;
            dataError = ApiErrors.Describe(response);
            dataState = LoadState.Failed;
        }

        private async Task OnProjectChanged(ChangeEventArgs e)
        {
            if (!Guid.TryParse(e.Value?.ToString(), out var id) || id == selectedId) return;

            selectedId = id;
            notice = null;
            await LoadDataAsync();
        }

        // ---- questions

        private static IEnumerable<Question> QuestionsOf(ModdedTemplate quiz) =>
            quiz.Questions?.OrderBy(q => q.SortOrder) ?? Enumerable.Empty<Question>();

        private static string ScoreText(User user) =>
            user.Score is null
                ? "–"
                : $"{user.Score.Correct} af {user.Score.TotalQuestions} · {Web.Helpers.Display.Percent(user.Score.Percentage)}";

        private async Task ToggleQuestionAsync(Question question)
        {
            if (!expanded.Add(question.Id))
            {
                expanded.Remove(question.Id);
                return;
            }

            if (questionStats.ContainsKey(question.Id)) return;

            loadingQuestions.Add(question.Id);
            questionErrors.Remove(question.Id);
            try
            {
                var response = await StatisticsApi.GetQuestionStatistics(question.Id);
                if (response.IsSuccessStatusCode && response.Content is not null)
                    questionStats[question.Id] = response.Content;
                else
                    questionErrors[question.Id] = ApiErrors.Describe(response);
            }
            catch (HttpRequestException)
            {
                questionErrors[question.Id] = ApiErrors.Offline;
            }
            finally
            {
                loadingQuestions.Remove(question.Id);
            }
        }

        // ---- dialogs

        private void AskClear(ModdedTemplate quiz)
        {
            activeQuiz = quiz;
            dialog = DialogKind.ClearBoard;
        }

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

        private async Task<string?> ClearBoardAsync() =>
            ApiErrors.ErrorOrNull(await UsersApi.DeleteModdedTemplateUsers(activeQuiz!.Id), ignoreNotFound: true);

        private async Task<string?> ResetQuestionAsync() =>
            ApiErrors.ErrorOrNull(await StatisticsApi.DeleteQuestionResults(activeQuestion!.Id), ignoreNotFound: true);

        private async Task<string?> DeleteResultsAsync() =>
            ApiErrors.ErrorOrNull(await StatisticsApi.DeleteProjectResults(selectedId!.Value), ignoreNotFound: true);

        private Task OnBoardCleared() => Finish("Highscores er ryddet.");

        private Task OnQuestionReset() => Finish("Svarene til spørgsmålet er nulstillet.");

        private Task OnResultsDeleted() => Finish("Alle resultater for udstillingen er slettet.");

        // Closes the dialog, tells the user what happened and reloads the numbers.
        private async Task Finish(string text)
        {
            CloseDialog();
            notice = new Notice("success", text);
            await LoadDataAsync();
        }
    }
}