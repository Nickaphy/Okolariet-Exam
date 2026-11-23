using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.ISessionRepository
{
    // The outbound port responsible for getting the session including its ProjectId
    public interface IGetSessionByIdAsync
    {
        Task<Session?> GetSessionByIdAsync(
            Guid sessionId,
            CancellationToken ct
        );
    }
}