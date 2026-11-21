using Application.Commands.ModdedTemplateCommands;   // UpdateModdedTemplateCommand
using Domain.Entities;                               // ModdedTemplate
using FluentResults;                                 // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface IUpdateModdedTemplateUseCase
    {
        Task<Result<ModdedTemplate>> UpdateAsync(UpdateModdedTemplateCommand command, CancellationToken ct);
    }
}