using Api.Helpers;
using Api.Responses.VersionResponses;
using Application.InBoundPort.IVersionUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// Controlleren modtager HTTP-kald, kalder det rette use case og oversætter resultatet til et HTTP-svar.
// Den indeholder ingen forretningsregler.
[ApiController]
// Alle endpoints kræver Admin-rollen, medmindre et endpoint markeres med [AllowAnonymous].
// {projectId:guid} i stien er id'et på det projekt, versionerne hører til. Stien skal matche Generated/Output.cs.
[Route("api/v1/projects/{projectId:guid}/versions")]
public class VersionsController(
    IListVersionsUseCase listUseCase,
    IPublishVersionUseCase publishUseCase,
    IGetLatestVersionUseCase latestUseCase,
    IGetDraftVersionUseCase draftUseCase) : ControllerBase
{
    // GET .../versions: historik over versioner.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VersionSummaryResponse>>> List(Guid projectId, CancellationToken ct)
    {
        var result = await listUseCase.ListAsync(projectId, ct);
        // ToProblem oversætter en fejl til den rette HTTP-status (fx NotFoundError -> 404).
        return result.IsFailed ? this.ToProblem(result) : Ok(result.Value.Select(VersionSummaryResponse.From));
    }

    // POST .../versions: publicér projektet.
    [HttpPost]
    public async Task<ActionResult<VersionSummaryResponse>> Publish(Guid projectId, CancellationToken ct)
    {
        var result = await publishUseCase.PublishAsync(projectId, ct);
        // 404 hvis projektet ikke findes. 422 hvis projektet ikke kan publiceres.
        if (result.IsFailed) return this.ToProblem(result);

        var response = VersionSummaryResponse.From(result.Value);
        // 201 Created med adressen på den nye version.
        return Created($"/api/v1/projects/{projectId}/versions/{response.VersionNumber}", response);
    }

    // GET .../versions/latest: seneste version. Åben for alle, fordi kiosken ikke er logget ind.
    [AllowAnonymous]
    [HttpGet("latest")]
    public async Task<ActionResult<PublishedVersionResponse>> GetLatest(Guid projectId, CancellationToken ct)
    {
        var result = await latestUseCase.GetLatestAsync(projectId, ct);
        if (result.IsFailed) return this.ToProblem(result);

        var v = result.Value;

        // ETag identificerer præcis denne version. Klienten sender den tilbage i headeren If-None-Match
        // ved næste kald. Er versionen uændret, svarer vi 304 uden indhold, hvilket sparer båndbredde.
        var etag = $"\"{v.ProjectId}-{v.VersionNumber}\"";
        if (Request.Headers.IfNoneMatch == etag) return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        // "no-cache" betyder ikke "gem ikke": browseren må genbruge svaret, men skal først spørge serveren,
        // om versionen er ændret.
        Response.Headers.CacheControl = "no-cache";
        return Ok(PublishedVersionResponse.From(v.ProjectId, v.VersionNumber, v.PublishedAt, v.Snapshot));
    }

    // GET .../versions/draft: forhåndsvisning. Kun Admin, da [AllowAnonymous] ikke er sat.
    [HttpGet("draft")]
    public async Task<ActionResult<PublishedVersionResponse>> GetDraft(Guid projectId, CancellationToken ct)
    {
        var result = await draftUseCase.GetDraftAsync(projectId, ct);

        // Versionsnummer 0 og ingen dato markerer, at det er en forhåndsvisning og ikke en gemt version.
        return result.IsFailed
            ? this.ToProblem(result)
            : Ok(PublishedVersionResponse.From(projectId, 0, null, result.Value));
    }
}