using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.IAssetRepository;
using Application.OutBoundPort.IQuestionRepository;
using Application.InBoundPort.IQuestionUseCases;
using Domain.Entities;
using FluentResults;
using Application.Commands.QuestionCommands;
using Application.ErrorMessages;

namespace Application.UseCases.Questions
{
    public class DeleteQuestionUseCase(
    IGetQuestionByIdAsync getQuestion,
    IUpdateQuestionAsync update) : IDeleteQuestionUseCase
    {
        public async Task<Result> DeleteAsync(DeleteQuestionCommand cmd, CancellationToken ct)
        {
            var question = await getQuestion.GetQuestionByIdAsync(cmd.QuestionId, ct);
            if (question is null || !question.IsActive)
                return Result.Fail(new NotFoundError("Question not found."));

            // soft delete: the row stays, so statistics survive
            question.Deactivate();
            await update.UpdateQuestionAsync(question, ct);
            return Result.Ok();
        }
    }
}