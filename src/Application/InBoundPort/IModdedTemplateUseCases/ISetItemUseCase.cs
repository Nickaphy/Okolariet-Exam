using Application.Commands.ModdedTemplateCommands;   // SetItemCommand
using Application.Helpers;                           // SetItemResult
using FluentResults;                                 // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface ISetItemUseCase
    {
        Task<Result<SetItemResult>> SetItemAsync(SetItemCommand command, CancellationToken ct);
    }
}