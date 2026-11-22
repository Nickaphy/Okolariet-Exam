using System.ComponentModel.DataAnnotations;
using Api.Helpers;
using Api.Responses.AssetResponses;
using Application.Commands.AssetCommands;
using Application.InBoundPort.IAssetUseCases;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Media files (images, video, audio) used by templates, questions and answers.
    // Everything is Admin-only, EXCEPT Get, because the kiosk has to load the media without logging in.
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/assets")]
    public class AssetsController(
        IListAssetsUseCase listAssetsUseCase,
        IUploadAssetUseCase uploadAssetUseCase,
        IGetAssetUseCase getAssetUseCase,
        IDeleteAssetUseCase deleteAssetUseCase) : ControllerBase
    {
        private const long MaxUploadBytes = 500_000_000; // The biggest kind (video). The use case checks the real limit per kind.

        [HttpGet] // Paged list for the editor's media picker.
        public async Task<ActionResult<PagedResponse<AssetResponse>>> List(
            [FromQuery] AssetKind? kind,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await listAssetsUseCase.ListAsync(kind, page, pageSize, ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var p = result.Value;
            return Ok(new PagedResponse<AssetResponse>(
                p.Items.Select(AssetResponse.From).ToList(), p.Page, p.PageSize, p.TotalCount));
        }

        [HttpPost] // Upload a file. It's sent as multipart/form-data, not JSON, so there's no Request record.
        [RequestSizeLimit(MaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
        public async Task<ActionResult<AssetResponse>> Upload(IFormFile file, CancellationToken ct) //The parameter MUST be called "file", because that's the name Refit sends
        {
            await using var stream = file.OpenReadStream();
            var result = await uploadAssetUseCase.UploadAsync(
                new UploadAssetCommand(stream, file.FileName, file.ContentType, file.Length), ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var response = AssetResponse.From(result.Value);
            return CreatedAtAction(nameof(Get), new { assetId = response.Id }, response);
        }

        [HttpGet("{assetId:guid}")] // Download/stream the file itself
        [AllowAnonymous]            // The kiosk isn't logged in, in this case
        public async Task<IActionResult> Get(Guid assetId, CancellationToken ct)
        {
            var result = await getAssetUseCase.GetAsync(assetId, ct);
            if (result.IsFailed)
                return this.ToProblem(result);

            var (asset, content) = result.Value; // Splits the tuple into two variables

            // From the remarks in Output.cs: files never change, so the browser may cache them "forever",
            // and nosniff stops the browser from guessing a different file type than the one we say.
            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            Response.Headers.XContentTypeOptions = "nosniff";

            // enableRangeProcessing handles the "Range" header for us, so video/audio can seek without downloading everything.
            // File(...) also closes the stream when it's done.
            return File(content, asset.ContentType, enableRangeProcessing: true);
        }

        [HttpDelete("{assetId:guid}")] // 409 if a question, answer or template still uses it
        public async Task<IActionResult> Delete(Guid assetId, CancellationToken ct)
        {
            var result = await deleteAssetUseCase.DeleteAsync(new DeleteAssetCommand(assetId), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }
    }
}
