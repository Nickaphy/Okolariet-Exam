using Web.Api.Contracts;

namespace Web.Ports.Answers
{
    public interface IAnswerGateway
    {
        Task<Answer> CreateAnswer(Guid questionId, AnswerInput body, CancellationToken cancellationToken = default);
        Task<Answer> UpdateAnswer(Guid answerId, AnswerInput body, CancellationToken cancellationToken = default);
        Task DeleteAnswer(Guid answerId, CancellationToken cancellationToken = default);
    }
}