using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.IAssetRepository;
using Application.OutBoundPort.IQuestionRepository;
using Application.InBoundPort.IAnswerUseCases;
using Domain.Entities;
using FluentResults;
using Application.Commands.AnswerCommands;
using Application.ErrorMessages;

namespace Application.UseCases.Answers
{
    public class DeleteAnswerUseCase(
    IGetQuestionByAnswerIdAsync getQuestionByAnswer,
    IUpdateQuestionAsync update) : IDeleteAnswerUseCase
    {
        public async Task<Result> DeleteAsync(DeleteAnswerCommand cmd, CancellationToken ct)
        {
            try
            {
                var question = await getQuestionByAnswer.GetQuestionByAnswerIdAsync(cmd.AnswerId, ct);
                var answer = question?.Answers.FirstOrDefault(a => a.Id == cmd.AnswerId && a.IsActive);
                if (question is null || answer is null)
                    return Result.Fail(new NotFoundError("Answer not found."));

                answer.Deactivate();
                await update.UpdateQuestionAsync(question, ct);
                return Result.Ok();
            }
            catch (ArgumentException ex)
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
            }
        }
    }
}