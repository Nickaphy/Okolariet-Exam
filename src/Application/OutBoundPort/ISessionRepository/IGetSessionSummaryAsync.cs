using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;

namespace Application.OutBoundPort.ISessionRepository
{
    public interface IGetSessionSummaryAsync
    {
        // GET /api/v1/sessions/{sessionId}
        //OutBound port implementing the SessionSummaryUsecase
        // Returns null when the session does not exist.
        Task<SessionSummary?> GetSummaryAsync(
            Guid sessionId,
            CancellationToken ct);
    }
}