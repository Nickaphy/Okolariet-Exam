using Domain.Entities;

namespace Application.OutBoundPort.IQuestionRepository
{
    public interface IGetQuestionByAnswerIdAsync
    {
        Task<Question?> GetQuestionByAnswerIdAsync(Guid answerId, CancellationToken ct);
    }
}
