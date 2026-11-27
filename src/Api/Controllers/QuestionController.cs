using Api.Helpers;
using Api.Requests.QuestionRequests;
using Api.Responses.QuestionResponses;
using Application.Commands.QuestionCommands;
using Application.InBoundPort.IQuestionUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1")]
public class QuestionsController(
    IListQuestionsUseCase listQuestionsUseCase,
    ICreateQuestionUseCase createQuestionUseCase,
    IGetQuestionUseCase getQuestionUseCase,
    IUpdateQuestionUseCase updateQuestionUseCase,
    IDeleteQuestionUseCase deleteQuestionUseCase) : ControllerBase
{
    [HttpGet("modded-templates/{moddedTemplateId:guid}/questions")]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> List(Guid moddedTemplateId, CancellationToken ct)
    {
        var result = await listQuestionsUseCase.ListAsync(moddedTemplateId, ct);
        return result.IsFailed ? this.ToProblem(result) : Ok(result.Value.Select(QuestionResponse.From));
    }

    [HttpPost("modded-templates/{moddedTemplateId:guid}/questions")]
    public async Task<ActionResult<QuestionResponse>> Create(Guid moddedTemplateId, [FromBody] QuestionRequest request, CancellationToken ct)
    {
        var result = await createQuestionUseCase.CreateAsync(new CreateQuestionCommand(moddedTemplateId, request.Text, request.AssetId, request.SortOrder), ct);
        if (result.IsFailed) return this.ToProblem(result);
        var response = QuestionResponse.From(result.Value);
        return CreatedAtAction(nameof(Get), new { questionId = response.Id }, response);
    }

    [HttpGet("questions/{questionId:guid}")]
    public async Task<ActionResult<QuestionResponse>> Get(Guid questionId, CancellationToken ct)
    {
        var result = await getQuestionUseCase.GetAsync(questionId, ct);
        return result.IsFailed ? this.ToProblem(result) : Ok(QuestionResponse.From(result.Value));
    }

    [HttpPut("questions/{questionId:guid}")]
    public async Task<ActionResult<QuestionResponse>> Update(Guid questionId, [FromBody] QuestionRequest request,
    [FromHeader(Name = "If-Match")] string ifMatch, CancellationToken ct)
    {
        var result = await updateQuestionUseCase.UpdateAsync(
            new UpdateQuestionCommand(questionId, request.Text, request.AssetId, request.SortOrder, ifMatch), ct);
        return result.IsFailed ? this.ToProblem(result) : Ok(QuestionResponse.From(result.Value));
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> Delete(Guid questionId, CancellationToken ct)
    {
        var result = await deleteQuestionUseCase.DeleteAsync(new DeleteQuestionCommand(questionId), ct);
        return result.IsFailed ? this.ToProblem(result) : NoContent();
    }
}