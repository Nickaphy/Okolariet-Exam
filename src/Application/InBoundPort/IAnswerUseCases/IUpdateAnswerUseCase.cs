using Application.Commands.AnswerCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAnswerUseCases
{
    public interface IUpdateAnswerUseCase
    {
        Task<Result<Answer>> UpdateAsync(UpdateAnswerCommand command, CancellationToken ct);
    }
}