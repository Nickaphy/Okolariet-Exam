using Domain.Entities;

namespace Application.OutBoundPort.IQuestionRepository
{
    public interface IGetQuestionByIdAsync
    {
        Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct);
    }
}
