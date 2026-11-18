using Application.Commands.QuestionCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuestionUseCases
{
    public interface IDeleteQuestionUseCase
    {
        Task<Result> DeleteAsync(DeleteQuestionCommand command, CancellationToken ct);
    }
}