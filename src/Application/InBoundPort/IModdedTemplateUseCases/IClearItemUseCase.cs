using Application.Commands.ModdedTemplateCommands;   // ClearItemCommand
using FluentResults;                                 // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface IClearItemUseCase
    {
        Task<Result> ClearItemAsync(ClearItemCommand command, CancellationToken ct);
    }
}