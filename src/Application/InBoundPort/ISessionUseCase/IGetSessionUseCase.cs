using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;
using FluentResults;

namespace Application.InBoundPort.ISessionUseCase
{
    // GET /api/v1/sessions/{sessionId}
    // The controller calls this port to request a session summary.
    // The Application use case implements it.
    public interface IGetSessionUseCase
    {
        Task<Result<SessionSummary>> GetAsync(
            Guid sessionId,
            CancellationToken ct);
    }
}