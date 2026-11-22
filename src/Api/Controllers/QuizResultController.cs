using Api.Helpers;
using Api.Requests.QuizResultRequest;
using Api.Responses.QuizResultResponses;
using Application.Commands.QuizResultCommands;
using Application.InBoundPort.IQuizResultUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/sessions/{sessionId:guid}/results")]
public class QuizResultsController(
    ISubmitQuizResultUseCase submitQuizResultUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<QuizResultResponse>> Submit(
        Guid sessionId,
        [FromBody] SubmitQuizResultRequest request,
        CancellationToken ct)
    {
        var command = new SubmitQuizResultCommand(
            sessionId,
            request.QuestionId,
            request.AnswerId);

        var result = await submitQuizResultUseCase.SubmitAsync(command, ct);

        if (result.IsFailed)
            return this.ToProblem(result);

        var submission = result.Value;
        var recordedResult = submission.RecordResult;

        var response = new QuizResultResponse(
            recordedResult.QuestionId,
            recordedResult.AnswerId,
            recordedResult.IsCorrect,
            submission.CorrectAnswerId);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}