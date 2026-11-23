using Api.Helpers;                                       // .ToProblem()
using Api.Requests.ModdedTemplateRequests;               // CreateModdedTemplateRequest, UpdateModdedTemplateRequest
using Api.Responses.ModdedTemplateResponses;             // ModdedTemplateResponse, ModdedTemplateSummaryResponse
using Application.Commands.ModdedTemplateCommands;       // the commands
using Application.InBoundPort.IModdedTemplateUseCases;   // the use case interfaces
using Microsoft.AspNetCore.Authorization;                // [Authorize]
using Microsoft.AspNetCore.Mvc;                          // ControllerBase, [HttpGet] ...

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1")]
    public class ModdedTemplatesController(
        IListModdedTemplatesUseCase listModdedTemplatesUseCase,
        ICreateModdedTemplateUseCase createModdedTemplateUseCase,
        IGetModdedTemplateUseCase getModdedTemplateUseCase,
        IUpdateModdedTemplateUseCase updateModdedTemplateUseCase,
        IDeleteModdedTemplateUseCase deleteModdedTemplateUseCase) : ControllerBase
    {
        [HttpGet("projects/{projectId:guid}/modded-templates")]
        public async Task<ActionResult<IEnumerable<ModdedTemplateSummaryResponse>>> List(Guid projectId, CancellationToken ct)
        {
            var result = await listModdedTemplatesUseCase.ListAsync(projectId, ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(result.Value.Select(ModdedTemplateSummaryResponse.From));
        }

        [HttpPost("projects/{projectId:guid}/modded-templates")]
        public async Task<ActionResult<ModdedTemplateResponse>> Create(Guid projectId, [FromBody] CreateModdedTemplateRequest request, CancellationToken ct)
        {
            var result = await createModdedTemplateUseCase.CreateAsync(
                new CreateModdedTemplateCommand(projectId, request.TemplateType, request.Title, request.Appearance?.ToInput(), request.SortOrder), ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var response = ModdedTemplateResponse.From(result.Value);
            return CreatedAtAction(nameof(Get), new { moddedTemplateId = response.Id }, response);
        }

        [HttpGet("modded-templates/{moddedTemplateId:guid}")]
        public async Task<ActionResult<ModdedTemplateResponse>> Get(Guid moddedTemplateId, CancellationToken ct)
        {
            var result = await getModdedTemplateUseCase.GetAsync(moddedTemplateId, ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var details = result.Value;
            return Ok(ModdedTemplateResponse.From(details.ModdedTemplate, details.Questions));   // the only endpoint that includes questions
        }

        [HttpPut("modded-templates/{moddedTemplateId:guid}")]
        public async Task<ActionResult<ModdedTemplateResponse>> Update(Guid moddedTemplateId, [FromBody] UpdateModdedTemplateRequest request,
            [FromHeader(Name = "If-Match")] string ifMatch, CancellationToken ct)
        {
            var result = await updateModdedTemplateUseCase.UpdateAsync(
                new UpdateModdedTemplateCommand(moddedTemplateId, request.Title, request.Appearance?.ToInput(), request.SortOrder, ifMatch), ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(ModdedTemplateResponse.From(result.Value));
        }

        [HttpDelete("modded-templates/{moddedTemplateId:guid}")]
        public async Task<IActionResult> Delete(Guid moddedTemplateId, CancellationToken ct)
        {
            var result = await deleteModdedTemplateUseCase.DeleteAsync(new DeleteModdedTemplateCommand(moddedTemplateId), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }
    }
}