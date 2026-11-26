using System.Globalization;
using Application.Commands.UserCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IUserUsecases;
using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.INicknameWordRepository;
using Application.OutBoundPort.IQuestionRepository;
using Application.OutBoundPort.IQuizResultRepository;
using Application.OutBoundPort.ISessionRepository;
using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.UseCases.Users;

// Creates a saved leaderboard entry for a completed quiz.
// Implements the inbound port that UserController calls.

// The constructor receives the outbound ports through dependency injection.
// Infrastructure supplies their implementations for retrieving and saving
public class CreateUserUseCase(
    ICreateUserAsync createUser, // Save the leaderboard entry
    IGetAsync getWord, // Retrieve the approved nickname
    IGetSessionByIdAsync getSession, // Retrieve the session
    ITemplateBelongsToProjectAsync templateBelongsToProject,
    IListQuestionsByTemplateAsync listQuestions,
    IListQuizResultsBySessionAsync listResults) : ICreateUserUseCase
{
    private static readonly SemaphoreSlim SaveLock = new(1, 1);

    public async Task<Result<User>> PostUserAsync(
        CreateUserCommand command,
        CancellationToken ct)
    {
        // 1. Validate the selected words and build the nickname.
        if (command.FirstWordId == command.SecondWordId)
            return Result.Fail<User>(
                new BusinessRuleError("Choose two different nickname words."));

        if (command.Number < 0 || command.Number > 99)
            return Result.Fail<User>(
                new BusinessRuleError("The nickname number must be 0 to 99."));

        var firstWord = await getWord.GetAsync(command.FirstWordId, ct);
        var secondWord = await getWord.GetAsync(command.SecondWordId, ct);

        if (firstWord is null || secondWord is null ||
            !firstWord.IsActive || !secondWord.IsActive)
        {
            return Result.Fail<User>(
                new BusinessRuleError("Choose two active approved words."));
        }

        var textInfo = CultureInfo.GetCultureInfo("da-DK").TextInfo;

        var nickname = textInfo.ToTitleCase(firstWord.Text)
            + textInfo.ToTitleCase(secondWord.Text)
            + command.Number.ToString(CultureInfo.InvariantCulture);

        // 2. Find the session and check the quiz's project.
        var session = await getSession.GetSessionByIdAsync(command.SessionId, ct);

        if (session is null)
            return Result.Fail<User>(
                new NotFoundError("Session not found."));

        var belongsToProject =
            await templateBelongsToProject.BelongsToProjectAsync(
                command.ModdedTemplateId, session.ProjectId, ct);

        if (!belongsToProject)
            return Result.Fail<User>(
                new BusinessRuleError(
                    "The quiz must belong to the session's project."));

        // 3. Retrieve the quiz questions and the session's results.
        var questions = await listQuestions.ListQuestionsByTemplateAsync(
            command.ModdedTemplateId, ct);

        var questionIds = questions
            .Where(question => question.IsActive)
            .Select(question => question.Id)
            .ToHashSet();

        if (questionIds.Count == 0)
            return Result.Fail<User>(
                new BusinessRuleError("The quiz has no active questions."));

        var results = await listResults.ListBySessionAsync(session.Id, ct);

        var quizResults = results
            .Where(result => questionIds.Contains(result.QuestionId))
            .ToList();

        // 4. Check completion and calculate the score.
        var answeredCount = quizResults
            .Select(result => result.QuestionId)
            .Distinct()
            .Count();

        if (answeredCount != questionIds.Count)
            return Result.Fail<User>(
                new BusinessRuleError(
                    "Answer every question before saving your score."));

        var correctCount = quizResults
            .Where(result => result.IsCorrect)
            .Select(result => result.QuestionId)
            .Distinct()
            .Count();

        var score = new Score(correctCount, questionIds.Count);

        // 5. Create and save the leaderboard entry.
        var user = User.Create(
            nickname,
            command.ModdedTemplateId,
            session.Id,
            score);

        // SemaphoreSlim Lock to ensure that only one leaderboard entry can be saved at a time.
        // This prevents race conditions where 2 entries see that the entry is not in the database and
        // both try to save it at the same time causing a dupelicate.
        await SaveLock.WaitAsync(ct);
        try
        {
            if (!await createUser.CreateUserAsync(user, ct))
                return Result.Fail<User>(
                    new ConflictError(
                        "This session has already saved a score for this quiz."));

            return Result.Ok(user);
        }
        finally
        {
            SaveLock.Release();
        }
}
