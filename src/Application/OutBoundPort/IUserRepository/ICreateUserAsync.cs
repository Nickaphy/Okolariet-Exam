using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.OutBoundPort.IUserRepository
{
    public interface ICreateUserAsync
    {
        // Contract for persisting the created user entity to the database using the outbound port
        // infra retruns true when saved, and false when the SessionId and ModdedTemplateId combination exists.
        Task<bool> CreateUserAsync(User user, CancellationToken ct);
    }
}