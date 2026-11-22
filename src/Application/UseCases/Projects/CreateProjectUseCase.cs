using System.Diagnostics;
using Application.Commands.ProjectCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IProjectUseCases;
using Application.OutBoundPort.IProjectRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Projects
{
    // Opretter et projekt. Use casen styrer rækkefølgen af trin og kontrollerer reglerne.
    // IAddProjectAsync leveres via dependency injection (primær konstruktør).
    public class CreateProjectUseCase(IAddProjectAsync add) : ICreateProjectUseCase
    {
        public async Task<Result<Project>> CreateAsync(CreateProjectCommand cmd, CancellationToken ct)
        {
            // Regel: et nyt projekt har endnu ingen skærme og kan derfor ikke have en startskærm.
            // Fejlen (BusinessRuleError) bliver til HTTP 422 i controlleren.
            if (cmd.MainModdedTemplateId is not null)
                return Result.Fail<Project>(new BusinessRuleError("A new project has no modded templates yet."));

            try
            {
                // Project.Create validerer navnet og kaster ArgumentException, hvis det er ugyldigt.
                var project = Project.Create(cmd.Name, cmd.Appearance);

                await add.AddAsync(project, ct);
                return Result.Ok(project);
            }
            catch (ArgumentException ex)
            {
                // Ugyldigt input fra domænet. Beskeden er sikker at vise for brugeren.
                return Result.Fail<Project>(new BusinessRuleError(ex.Message));
            }
            catch (Exception ex)
            {
                // Uventede fejl (fx databasen er utilgængelig) logges, men detaljerne vises ikke for brugeren,
                // da de kan indeholde interne oplysninger.
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail<Project>(new UnexpectedError("An unexpected error occurred while creating the project."));
            }
        }
    }
}