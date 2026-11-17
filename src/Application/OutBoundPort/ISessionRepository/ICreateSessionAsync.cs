using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.ISessionRepository
{
    public interface ICreateSessionAsync
    {
        // Contract responsible for getting our fresh and frighty user session persisted
        Task CreateSessionAsync(Session session, CancellationToken ct);
    }
}