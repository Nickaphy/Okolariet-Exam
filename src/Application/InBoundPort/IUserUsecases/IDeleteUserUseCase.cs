using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;

namespace Application.InBoundPort.IUserUsecases
{
    // Inbound port to delete a leaderboard entry (user)
    public interface IDeleteUserUseCase
    {
        Task<Result> DeleteAsync(Guid userId, CancellationToken ct);
    }
}