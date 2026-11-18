using Application.Helpers;
using Domain.Entities;

namespace Application.OutBoundPort.IProjectRepository
{
    // Outbound ports: det, use casene har brug for fra omverdenen (database m.m.).
    // Application definerer interfaces her, og Infrastructure implementerer dem senere.
    // Dermed afhænger forretningslogikken ikke af, hvordan data konkret gemmes.
    // Der er ét interface pr. handling, som i Staff og Nickname.

    // Gemmer et nyt projekt.
    public interface IAddProjectAsync { Task AddAsync(Project project, CancellationToken ct); }

    // Finder et projekt ud fra id. Returnerer null, hvis det ikke findes.
    public interface IGetProjectByIdAsync { Task<Project?> GetByIdAsync(Guid id, CancellationToken ct); }

    // Henter én side af projektlisten (paging).
    public interface IListProjectsAsync { Task<PagedList<Project>> ListAsync(int page, int pageSize, CancellationToken ct); }

    // Gemmer ændringer til et eksisterende projekt.
    public interface IUpdateProjectAsync { Task UpdateAsync(Project project, CancellationToken ct); }

    // Sletter et projekt. Tilhørende skærme og versioner skal også slettes (cascade),
    // hvilket håndteres i Infrastructure.
    public interface IDeleteProjectAsync { Task DeleteAsync(Project project, CancellationToken ct); }

    // Kontrollerer, om en skærm (ModdedTemplate) hører til et givent projekt.
    // Bruges til reglen om, at startskærmen skal tilhøre projektet.
    // Kan først implementeres, når ModdedTemplate findes i databasen, og implementeres af den, der laver den.
    public interface IModdedTemplateBelongsToProjectAsync
    {
        Task<bool> BelongsToProjectAsync(Guid moddedTemplateId, Guid projectId, CancellationToken ct);
    }
}