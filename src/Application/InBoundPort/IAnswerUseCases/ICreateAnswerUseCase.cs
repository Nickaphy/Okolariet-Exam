using Application.Commands.AnswerCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAnswerUseCases
{
    public interface ICreateAnswerUseCase
    {
        Task<Result<Answer>> CreateAsync(CreateAnswerCommand command, CancellationToken ct);
    }
}