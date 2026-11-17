using Application.Commands.QuestionCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IQuestionUseCases
{
    public interface IUpdateQuestionUseCase
    {
        Task<Result<Question>> UpdateAsync(UpdateQuestionCommand command, CancellationToken ct);
    }
}