using Application.Commands.QuizResultCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IQuizResultUseCases;
using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.IQuestionRepository;
using Application.OutBoundPort.IQuizResultRepository;
using Application.OutBoundPort.ISessionRepository;
using Domain.Entities;
using Application;
using FluentResults;

namespace Application.UseCases.QuizResults;

public class SubmitQuizResultUseCase(
    IGetSessionByIdAsync getSession,
    IGetQuestionByIdAsync getQuestion,
    ITemplateBelongsToProjectAsync templateBelongsToProject,
    ITryAddQuizResultAsync addResult) : ISubmitQuizResultUseCase
{
    public async Task<Result<SubmittedQuizAnswer>> SubmitAsync(
        SubmitQuizResultCommand command, // recieve the guest selection
        CancellationToken ct)
    {
        // Get the current session  
        var session = await getSession.GetSessionByIdAsync(command.SessionId, ct);

        if (session is null)
            return Result.Fail<SubmittedQuizAnswer>(
                new NotFoundError("Session not found."));

        // Get the question
        var question = await getQuestion.GetQuestionByIdAsync(command.QuestionId, ct);

        if (question is null)
            return Result.Fail<SubmittedQuizAnswer>(
                new NotFoundError("Question not found."));

        if (!question.IsActive)
            return Result.Fail<SubmittedQuizAnswer>(
                new BusinessRuleError("The question is inactive."));

        // does the question belong to the project?
        var belongsToProject = await templateBelongsToProject.BelongsToProjectAsync(
            question.ModdedTemplateId, session.ProjectId, ct);

        if (!belongsToProject)
            return Result.Fail<SubmittedQuizAnswer>(
                new BusinessRuleError(
                    "The question must belong to the session's project."));

        // Find the selected answer inside the question
        var answer = question.Answers.FirstOrDefault(
            answer => answer.Id == command.AnswerId && answer.IsActive);

        if (answer is null)
            return Result.Fail<SubmittedQuizAnswer>(
                new BusinessRuleError(
                    "Select an active answer belonging to this question."));

        // Find the single active correct answer before saving.
        var correctAnswers = question.Answers
            .Where(answer => answer.IsActive && answer.IsCorrect)
            .ToList();

        if (correctAnswers.Count != 1)
            return Result.Fail<SubmittedQuizAnswer>(
                new BusinessRuleError(
                    "The question must have exactly one active correct answer."));

        var correctAnswerId = correctAnswers[0].Id;

        // Create the recorded result in the domain entity
        var quizResult = QuizResult.Create(session.Id, question, answer);

        // save it while preventing duplicate submissions
        if (!await addResult.TryAddAsync(quizResult, ct))
            return Result.Fail<SubmittedQuizAnswer>(
                new ConflictError(
                    "This session has already answered this question."));

        // Return success to the controller
        return Result.Ok(
            new SubmittedQuizAnswer(quizResult, correctAnswerId));
    }
}