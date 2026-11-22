using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.OutBoundPort.ISessionRepository
{
    public interface IProjectExists
    {
        // Outbound port responsible for getting the existence of a project from the infrastructure back to AddSessionUsecase.
        Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct);
    }
}