using System.ComponentModel.DataAnnotations;
using Api.Helpers;
using Api.Requests.NicknameRequests;
using Api.Responses.NicknameResponse;
using Application.Commands.NicknameCommands;
using Application.InBoundPort.INicknameUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[AllowAnonymous]  // Gets the 10 random words (suggestions) so the user can see them without being logged in.
[Route("api/v1/nickname-suggestions")]
public class NicknameSuggestionsController(IGetSuggestionsAsync useCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NicknameWordResponse>>> Get(CancellationToken ct)
    {
        var result = await useCase.GetSuggestionsAsync(ct);
        if (result.IsFailed) 
            return this.ToProblem(result);

        return Ok(result.Value.Select(NicknameWordResponse.From));
    }
}

[ApiController]
[Route("api/v1/nickname-words")]
public class NicknameWordsController(IListWordsAsync listWordsUseCase, IAddWordAsync addWordUseCase, IDeleteWordAsync deleteWordUseCase) : ControllerBase
{
    [HttpGet] // List all words
    public async Task<ActionResult<PagedResponse<NicknameWordResponse>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await listWordsUseCase.ListWordsAsync(page, pageSize, ct);
        if (result.IsFailed) 
            return this.ToProblem(result);

        var p = result.Value;
        return Ok(new PagedResponse<NicknameWordResponse>(
            p.Items.Select(NicknameWordResponse.From).ToList(), p.Page, p.PageSize, p.TotalCount));
    }

    [HttpPost] // "Create" or more like add a new word to the lists of possible words
    public async Task<ActionResult<NicknameWordResponse>> Create(
        [FromBody] NicknameWordRequest request, CancellationToken ct)
    {
        var result = await addWordUseCase.AddWordAsync(new AddNicknameWordCommand(request.Text), ct); //Overkill to use command, but keeps the system consistent. 
        if (result.IsFailed) 
            return this.ToProblem(result);          // 409 if it already exists

        var response = NicknameWordResponse.From(result.Value);
        return Created($"/api/v1/nickname-words/{response.Id}", response);
    }

    [HttpDelete("{nicknameWordId:guid}")] // Delete a word from the list of possible words
    public async Task<IActionResult> Delete(Guid nicknameWordId, CancellationToken ct)
    {
        var result = await deleteWordUseCase.DeleteWordAsync(new DeleteNicknameWordCommand(nicknameWordId), ct); //Also overkill, kept as command for consistency
        return result.IsFailed ? this.ToProblem(result) : NoContent();
    }
}