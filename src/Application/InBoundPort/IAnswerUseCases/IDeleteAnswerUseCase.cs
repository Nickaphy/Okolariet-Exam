using Application.Commands.AnswerCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAnswerUseCases
{
    public interface IDeleteAnswerUseCase
    {
        Task<Result> DeleteAsync(DeleteAnswerCommand command, CancellationToken ct);
    }
}