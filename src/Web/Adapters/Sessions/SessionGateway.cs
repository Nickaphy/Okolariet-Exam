using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Sessions;

namespace Web.Adapters.Sessions
{
    public class SessionGateway(ISessionsApi api) : ISessionGateway
    {
        public async Task<Session> CreateSession(Guid projectId, CancellationToken ct = default)
            => (await api.CreateSession(new SessionInput { ProjectId = projectId }, ct)).Unwrap();

        public async Task<QuizResult> SubmitResult(Guid sessionId, Guid questionId, Guid answerId, CancellationToken ct = default)
            => (await api.SubmitResult(sessionId, new QuizResultInput { QuestionId = questionId, AnswerId = answerId }, ct)).Unwrap();

        public async Task<SessionSummary> GetSession(Guid sessionId, CancellationToken ct = default)
            => (await api.GetSession(sessionId, ct)).Unwrap();
    }
}
