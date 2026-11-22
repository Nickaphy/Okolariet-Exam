using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Answers;

namespace Web.Adapters.Answers
{
    public class AnswerGateway(IAnswersApi api) : IAnswerGateway
    {
        public async Task<Answer> CreateAnswer(Guid questionId, AnswerInput body, CancellationToken cancellationToken = default)
            => (await api.CreateAnswer(questionId, body, cancellationToken)).Unwrap();

        public async Task<Answer> UpdateAnswer(Guid answerId, AnswerInput body, CancellationToken cancellationToken = default)
            => (await api.UpdateAnswer(answerId, body, cancellationToken)).Unwrap();

        public async Task DeleteAnswer(Guid answerId, CancellationToken cancellationToken = default)
            => (await api.DeleteAnswer(answerId, cancellationToken)).EnsureSuccess();
    }
}