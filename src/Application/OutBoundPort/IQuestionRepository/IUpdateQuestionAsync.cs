using Domain.Entities;

namespace Application.OutBoundPort.IQuestionRepository
{
    public interface IUpdateQuestionAsync
    {
        Task UpdateQuestionAsync(Question question, CancellationToken ct);
    }
}
