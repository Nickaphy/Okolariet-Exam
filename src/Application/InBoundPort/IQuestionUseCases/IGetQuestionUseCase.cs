using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuestionUseCases
{
    public interface IGetQuestionUseCase
    {
        Task<Result<Question>> GetAsync(Guid id, CancellationToken ct);
    }
}