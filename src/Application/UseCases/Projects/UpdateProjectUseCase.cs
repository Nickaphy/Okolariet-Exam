using System.Diagnostics;
using Application.Commands.ProjectCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IProjectUseCases;
using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Projects
{
    // Opdaterer navn, startskærm og udseende på et projekt.
    // Afhængigheder: getById finder projektet, belongs kontrollerer at startskærmen tilhører projektet,
    // og update gemmer ændringerne.
    public class UpdateProjectUseCase(
        IGetProjectByIdAsync getById,
        IModdedTemplateBelongsToProjectAsync belongs,
        IUpdateProjectAsync update) : IUpdateProjectUseCase
    {
        public async Task<Result<Project>> UpdateAsync(UpdateProjectCommand cmd, CancellationToken ct)
        {
            try
            {
                // Projektet skal findes. Ellers HTTP 404.
                var project = await getById.GetByIdAsync(cmd.Id, ct);
                if (project is null) return Result.Fail<Project>(new NotFoundError("Project not found."));

                // Er der valgt en startskærm, skal den tilhøre dette projekt. Ellers kunne man vælge en
                // skærm fra en anden udstilling. Fejl giver HTTP 422.
                // "is Guid templateId" kontrollerer, at værdien ikke er null, og pakker den ud
                // i variablen templateId. "!" foran betyder "ikke".
                if (cmd.MainModdedTemplateId is Guid templateId
                    && !await belongs.BelongsToProjectAsync(templateId, project.Id, ct))
                    return Result.Fail<Project>(new BusinessRuleError("mainModdedTemplateId must belong to this project."));

                // Opdater domæneobjektet. Navnet valideres igen i Update.
                project.Update(cmd.Name, cmd.MainModdedTemplateId, cmd.Appearance);

                // Gem og returnér det opdaterede projekt.
                await update.UpdateAsync(project, ct);
                return Result.Ok(project);
            }
            catch (ArgumentException ex) { return Result.Fail<Project>(new BusinessRuleError(ex.Message)); } // ugyldigt navn
            catch (Exception ex)
            {
                // Uventede fejl logges, men detaljerne vises ikke for brugeren.
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail<Project>(new UnexpectedError("An unexpected error occurred while updating the project."));
            }
        }
    }
}