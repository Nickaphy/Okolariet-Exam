using Application.ErrorMessages;                            // NotFoundError
using Application.InBoundPort.IModdedTemplateUseCases;      // IListModdedTemplatesUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IListModdedTemplatesByProjectAsync
using Application.OutBoundPort.IProjectRepository;          // IProjectExistsAsync
using Domain.Entities;                                      // ModdedTemplate
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class ListModdedTemplatesUseCase(
        IProjectExistsAsync projectExists,
        IListModdedTemplatesByProjectAsync listModdedTemplates) : IListModdedTemplatesUseCase
    {
        public async Task<Result<IReadOnlyList<ModdedTemplate>>> ListAsync(Guid projectId, CancellationToken ct)
        {
            // An unknown project is 404. An EXISTING project with no modded templates is just an empty list.
            if (!await projectExists.ProjectExistsAsync(projectId, ct))
                return Result.Fail(new NotFoundError("Project not found."));

            var moddedTemplates = await listModdedTemplates.ListModdedTemplatesByProjectAsync(projectId, ct);
            return Result.Ok<IReadOnlyList<ModdedTemplate>>(moddedTemplates.OrderBy(m => m.SortOrder).ToList());   
            // "ordered by sortOrder"
        }
    }
}