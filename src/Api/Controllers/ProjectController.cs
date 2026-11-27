using System.ComponentModel.DataAnnotations;
using Api.Helpers;
using Api.Requests.ProjectRequests;
using Api.Responses.ProjectResponses;
using Application.Commands.ProjectCommands;
using Application.InBoundPort.IProjectUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Controlleren modtager HTTP-kald, kalder det rette use case og oversætter resultatet til et HTTP-svar.
// Den indeholder ingen forretningsregler.
[ApiController]                // aktiverer automatisk validering af Request-klasserne (HTTP 400 ved fejl)
[Route("api/v1/projects")]     // grundstien. Skal matche stierne i Generated/Output.cs
public class ProjectsController(
    IListProjectsUseCase listUseCase,
    IGetProjectUseCase getUseCase,
    ICreateProjectUseCase createUseCase,
    IUpdateProjectUseCase updateUseCase,
    IDeleteProjectUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProjectResponse>>> List(
        // [FromQuery] = værdien læses fra adressens query string.
        // [Range] begrænser værdierne: page mindst 1, pageSize mellem 1 og 100.
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await listUseCase.ListAsync(page, pageSize, ct);

        // ToProblem oversætter en fejl til den rette HTTP-status (fx NotFoundError -> 404).
        if (result.IsFailed) return this.ToProblem(result);

        // Hvert projekt konverteres til et svar (Select), og siden returneres med HTTP 200.
        var p = result.Value;
        return Ok(new PagedResponse<ProjectResponse>(
            p.Items.Select(ProjectResponse.From).ToList(), p.Page, p.PageSize, p.TotalCount));
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectResponse>> Get(Guid projectId, CancellationToken ct)
    {
        var result = await getUseCase.GetAsync(projectId, ct);
        return result.IsFailed ? this.ToProblem(result) : Ok(ProjectResponse.From(result.Value));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create([FromBody] ProjectRequest request, CancellationToken ct)
    {
        // [FromBody] = værdien læses fra request-kroppen (JSON).
        // Request konverteres til et command, så Application ikke afhænger af Api's typer.
        // "request.Appearance?.ToDomain()" giver null, hvis der ikke er sendt et udseende.
        var cmd = new CreateProjectCommand(request.Name, request.MainModdedTemplateId, request.Appearance?.ToDomain());
        var result = await createUseCase.CreateAsync(cmd, ct);
        if (result.IsFailed) return this.ToProblem(result);

        var response = ProjectResponse.From(result.Value);

        // HTTP 201 Created med adressen på det nye projekt.
        return Created($"/api/v1/projects/{response.Id}", response);
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid projectId, [FromBody] ProjectRequest request, CancellationToken ct)
    {
        var cmd = new UpdateProjectCommand(projectId, request.Name, request.MainModdedTemplateId, request.Appearance?.ToDomain());
        var result = await updateUseCase.UpdateAsync(cmd, ct);
        return result.IsFailed ? this.ToProblem(result) : Ok(ProjectResponse.From(result.Value));
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken ct)
    {
        var result = await deleteUseCase.DeleteAsync(new DeleteProjectCommand(projectId), ct);

        // HTTP 204 No Content: slettet, og der er intet indhold at returnere.
        return result.IsFailed ? this.ToProblem(result) : NoContent();
    }
}