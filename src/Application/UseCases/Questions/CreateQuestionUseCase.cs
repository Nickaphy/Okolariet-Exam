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
    public class CreateQuestionUseCase(
        ITemplateSupportsQuestionsAsync supportsQuestions,
        IAssetExistsAsync assetExists,
        IAddQuestionAsync add) : ICreateQuestionUseCase
    {
        public async Task<Result<Question>> CreateAsync(CreateQuestionCommand cmd, CancellationToken ct)
        {
            try
            {
                //422
                if (!await supportsQuestions.SupportsQuestionsAsync(cmd.ModdedTemplateId, ct))
                    return Result.Fail(new BusinessRuleError("This template does not support questions."));  // 422
                
                //404
                if (cmd.AssetId is { } assetId && !await assetExists.AssetExistsAsync(assetId, ct))
                    return Result.Fail(new NotFoundError("Asset not found."));

                //domain validation
                var question = Question.Create(cmd.ModdedTemplateId, cmd.Text, cmd.AssetId, cmd.SortOrder);
                await add.AddQuestionAsync(question, ct);
                return Result.Ok(question);
            }
            catch (ArgumentException ex)   // validation failed in the domain
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
            }
        }
    }
}