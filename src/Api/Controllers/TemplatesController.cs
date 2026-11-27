using Api.Helpers;                                // .ToProblem()
using Api.Responses.TemplateResponses;            // TemplateResponse
using Application.InBoundPort.ITemplateUseCases;  // IListTemplatesUseCase, IGetTemplateUseCase
using Microsoft.AspNetCore.Authorization;         // [Authorize]
using Microsoft.AspNetCore.Mvc;                   // ControllerBase, [HttpGet] ...

namespace Api.Controllers
{
    // Read-only: the editor fetches the template definitions and builds its forms from them.
    [ApiController]
    [Route("api/v1/templates")]
    public class TemplatesController(
        IListTemplatesUseCase listTemplatesUseCase,
        IGetTemplateUseCase getTemplateUseCase) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateResponse>>> List(CancellationToken ct)
        {
            var result = await listTemplatesUseCase.ListAsync(ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(result.Value.Select(TemplateResponse.From));
        }

        [HttpGet("{templateType}")]   // No :guid here, because the key is text like "menu-buttons"
        public async Task<ActionResult<TemplateResponse>> Get(string templateType, CancellationToken ct)
        {
            var result = await getTemplateUseCase.GetAsync(templateType, ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(TemplateResponse.From(result.Value));
        }
    }
}