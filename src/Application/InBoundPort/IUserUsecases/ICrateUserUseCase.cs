using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Domain.Entities;
using Application.Commands.UserCommands;

namespace Application.InBoundPort.IUserUsecases
{
    public interface ICreateUserUseCase
    {
        Task<Result<User>> PostUserAsync(
        CreateUserCommand command,
        CancellationToken ct);
    }
}