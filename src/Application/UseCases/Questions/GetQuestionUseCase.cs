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
    public class GetQuestionUseCase(IGetQuestionByIdAsync getQuestion) : IGetQuestionUseCase
    {
        public async Task<Result<Question>> GetAsync(Guid id, CancellationToken ct)
        {
            var question = await getQuestion.GetQuestionByIdAsync(id, ct);
            if (question is null || !question.IsActive)
                return Result.Fail(new NotFoundError("Question not found."));

            return Result.Ok(question);
        }
    }
}