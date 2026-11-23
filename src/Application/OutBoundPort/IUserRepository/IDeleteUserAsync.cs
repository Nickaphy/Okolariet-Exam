using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.OutBoundPort.IUserRepository
{
    public interface IDeleteUserAsync
    {
        // outbound port for deleting an entry
        // Return false when no entry with this ID exists
        Task<bool> DeleteAsync(Guid userId, CancellationToken ct);
    }
} 