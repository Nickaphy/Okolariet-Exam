using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IUserUsecases
{
    // The inbound port implemented by Application
    // Getting the User
    public interface IGetUserUseCase
    {
        Task<Result<User>> GetAsync(Guid userId, CancellationToken ct);
    }
}