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
    public class CreateAnswerUseCase(
    IGetQuestionByIdAsync getQuestion,
    IAssetExistsAsync assetExists,
    IUpdateQuestionAsync update) : ICreateAnswerUseCase
    {
        public async Task<Result<Answer>> CreateAsync(CreateAnswerCommand cmd, CancellationToken ct)
        {
            try
            {
                var question = await getQuestion.GetQuestionByIdAsync(cmd.QuestionId, ct);
                if (question is null || !question.IsActive)
                    return Result.Fail(new NotFoundError("Question not found."));

                if (cmd.AssetId is { } assetId && !await assetExists.AssetExistsAsync(assetId, ct))
                    return Result.Fail(new NotFoundError("Asset not found."));

                var answer = question.AddAnswer(cmd.Text, cmd.IsCorrect, cmd.AssetId, cmd.SortOrder);
                await update.UpdateQuestionAsync(question, ct);   // saves the question AND the new answer
                return Result.Ok(answer);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
            }
        }
    }
}