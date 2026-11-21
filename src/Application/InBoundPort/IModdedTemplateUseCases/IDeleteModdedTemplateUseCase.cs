using Application.Commands.ModdedTemplateCommands;   // DeleteModdedTemplateCommand
using FluentResults;                                 // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface IDeleteModdedTemplateUseCase
    {
        Task<Result> DeleteAsync(DeleteModdedTemplateCommand command, CancellationToken ct);
    }
}