using Application.Commands.QuestionCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuestionUseCases
{
    public interface ICreateQuestionUseCase
    {
        Task<Result<Question>> CreateAsync(CreateQuestionCommand command, CancellationToken ct);
    }
}