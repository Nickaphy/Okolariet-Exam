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
    public class UpdateQuestionUseCase(
    IGetQuestionByIdAsync getQuestion,
    IAssetExistsAsync assetExists,
    IUpdateQuestionAsync update) : IUpdateQuestionUseCase
{
    public async Task<Result<Question>> UpdateAsync(UpdateQuestionCommand cmd, CancellationToken ct)
    {
        try
        {
            // 404
            var question = await getQuestion.GetQuestionByIdAsync(cmd.QuestionId, ct);
            if (question is null || !question.IsActive)
                return Result.Fail(new NotFoundError("Question not found."));

            // 404
            if (cmd.AssetId is { } assetId && !await assetExists.AssetExistsAsync(assetId, ct))
                return Result.Fail(new NotFoundError("Asset not found."));

            // TODO: check cmd.IfMatch against the question's version once Infrastructure has RowVersion
            
            // domain validation
            question.Update(cmd.Text, cmd.AssetId, cmd.SortOrder);
            await update.UpdateQuestionAsync(question, ct);
            return Result.Ok(question);
        }
        catch (ArgumentException ex)
        {
            // 422
            return Result.Fail(new BusinessRuleError(ex.Message));
        }
    }
}
}