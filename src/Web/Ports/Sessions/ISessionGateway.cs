using Web.Api.Contracts;

namespace Web.Ports.Sessions
{
    // A guest playing at the kiosk (anonymous). A session is started when a quiz starts, every answer is
    // sent to it, and the server says whether it was right (the published snapshot never contains the right answers).
    public interface ISessionGateway
    {
        Task<Session> CreateSession(Guid projectId, CancellationToken ct = default);
        Task<QuizResult> SubmitResult(Guid sessionId, Guid questionId, Guid answerId, CancellationToken ct = default);
        Task<SessionSummary> GetSession(Guid sessionId, CancellationToken ct = default);
    }
}
