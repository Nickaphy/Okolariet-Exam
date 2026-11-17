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
    public class UpdateAnswerUseCase(
    IGetQuestionByAnswerIdAsync getQuestionByAnswer,
    IAssetExistsAsync assetExists,
    IUpdateQuestionAsync update) : IUpdateAnswerUseCase
    {
        public async Task<Result<Answer>> UpdateAsync(UpdateAnswerCommand cmd, CancellationToken ct)
        {
            try
            {
                var question = await getQuestionByAnswer.GetQuestionByAnswerIdAsync(cmd.AnswerId, ct);
                var answer = question?.Answers.FirstOrDefault(a => a.Id == cmd.AnswerId && a.IsActive);
                if (question is null || answer is null)
                    return Result.Fail(new NotFoundError("Answer not found."));

                if (cmd.AssetId is { } assetId && !await assetExists.AssetExistsAsync(assetId, ct))
                    return Result.Fail(new NotFoundError("Asset not found."));

                answer.Update(cmd.Text, cmd.IsCorrect, cmd.AssetId, cmd.SortOrder);
                await update.UpdateQuestionAsync(question, ct);
                return Result.Ok(answer);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
            }
        }
    }
}