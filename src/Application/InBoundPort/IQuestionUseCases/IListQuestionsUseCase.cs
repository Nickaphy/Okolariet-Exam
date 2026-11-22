using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuestionUseCases
{
    public interface IListQuestionsUseCase
    {
        Task<Result<IReadOnlyList<Question>>> ListAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}