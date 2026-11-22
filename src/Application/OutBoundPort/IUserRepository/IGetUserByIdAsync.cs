using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.IUserRepository
{
    // Outbound port to be implemented in the infrastructure to get a User
    public interface IGetUserByIdAsync
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
    }
}