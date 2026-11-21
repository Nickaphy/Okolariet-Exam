using Api.Helpers;                                       // .ToProblem()
using Api.Requests.ItemRequests;                         // SetItemRequest
using Api.Responses.ModdedTemplateResponses;             // ModdedTemplateItemResponse
using Application.Commands.ModdedTemplateCommands;       // SetItemCommand, ClearItemCommand
using Application.InBoundPort.IModdedTemplateUseCases;   // ISetItemUseCase, IClearItemUseCase
using Microsoft.AspNetCore.Authorization;                // [Authorize]
using Microsoft.AspNetCore.Mvc;                          // ControllerBase, [HttpPut] ...

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/modded-templates/{moddedTemplateId:guid}/items")]
    public class ItemsController(
        ISetItemUseCase setItemUseCase,
        IClearItemUseCase clearItemUseCase) : ControllerBase
    {
        [HttpPut("{slotKey}")]   // NO ":guid", because slotKey is text like "button1"
        public async Task<ActionResult<ModdedTemplateItemResponse>> SetItem(Guid moddedTemplateId, string slotKey,
            [FromBody] SetItemRequest request, CancellationToken ct)
        {
            var result = await setItemUseCase.SetItemAsync(
                new SetItemCommand(moddedTemplateId, slotKey, request.Text, request.AssetId, request.TargetModdedTemplateId), ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var response = ModdedTemplateItemResponse.From(result.Value.Item);
            if (result.Value.Created)
                return Created($"/api/v1/modded-templates/{moddedTemplateId}/items/{slotKey}", response);
                // 201: new item

            return Ok(response);
            // 200: replaced
        }

        [HttpDelete("{slotKey}")]
        public async Task<IActionResult> ClearItem(Guid moddedTemplateId, string slotKey, CancellationToken ct)
        {
            var result = await clearItemUseCase.ClearItemAsync(new ClearItemCommand(moddedTemplateId, slotKey), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }
    }
}