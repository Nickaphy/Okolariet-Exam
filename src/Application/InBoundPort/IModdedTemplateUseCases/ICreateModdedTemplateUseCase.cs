using Application.Commands.ModdedTemplateCommands;   // CreateModdedTemplateCommand
using Domain.Entities;                               // ModdedTemplate
using FluentResults;                                 // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface ICreateModdedTemplateUseCase
    {
        Task<Result<ModdedTemplate>> CreateAsync(CreateModdedTemplateCommand command, CancellationToken ct);
    }
}