using Domain.Entities;

namespace Application.OutBoundPort.IQuestionRepository
{
    public interface IAddQuestionAsync
    {
        Task AddQuestionAsync(Question question, CancellationToken ct);
    }
}
