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
    public class ListQuestionsUseCase(IListQuestionsByTemplateAsync listQuestions) : IListQuestionsUseCase
    {
        public async Task<Result<IReadOnlyList<Question>>> ListAsync(Guid moddedTemplateId, CancellationToken ct)
        {
            var questions = await listQuestions.ListQuestionsByTemplateAsync(moddedTemplateId, ct);
            return Result.Ok<IReadOnlyList<Question>>(questions.Where(q => q.IsActive).ToList());
        }
    }
}