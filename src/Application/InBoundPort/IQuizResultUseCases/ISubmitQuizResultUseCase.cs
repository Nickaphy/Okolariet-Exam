using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.QuizResultCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuizResultUseCases
{
    // The inbound port implemented by the application
    // The command carries the guest selection
    public interface ISubmitQuizResultUseCase
    {
        Task<Result<SubmittedQuizAnswer>> SubmitAsync(
            SubmitQuizResultCommand command,
            CancellationToken ct);
    }
}