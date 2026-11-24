using Application.Models;
using Application.OutBoundPort.ISessionRepository;
using Domain.Entities;

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeSessionRepository :
        IProjectExists, ICreateSessionAsync, IGetSessionByIdAsync, IGetSessionSummaryAsync
    {
        public Guid ProjectId { get; } = Guid.NewGuid();
        public Session? Session { get; set; }
        public SessionSummary? Summary { get; set; }

        public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult(projectId == ProjectId);

        public Task CreateSessionAsync(Session session, CancellationToken ct)
        {
            Session = session;
            return Task.CompletedTask;
        }

        public Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct)
            => Task.FromResult(Session?.Id == sessionId ? Session : null);

        public Task<SessionSummary?> GetSummaryAsync(Guid sessionId, CancellationToken ct)
            => Task.FromResult(Summary?.Id == sessionId ? Summary : null);
    }
}
