using Api.Helpers;
using Api.Requests.AnswerRequests;
using Api.Responses.AnswerResponses;
using Application.Commands.AnswerCommands;
using Application.InBoundPort.IAnswerUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Answers always belong to a question. Creating one goes through /questions/{id}/answers,
    // while updating and deleting use the answer's own id (/answers/{id}).
    [ApiController]
    [Route("api/v1")]
    public class AnswersController(
        ICreateAnswerUseCase createAnswerUseCase,
        IUpdateAnswerUseCase updateAnswerUseCase,
        IDeleteAnswerUseCase deleteAnswerUseCase) : ControllerBase
    {
        [HttpPost("questions/{questionId:guid}/answers")] //Add an answer to a question
        public async Task<ActionResult<AnswerResponse>> Create(Guid questionId, [FromBody] AnswerRequest request, CancellationToken ct)
        {
            var result = await createAnswerUseCase.CreateAsync(
                new CreateAnswerCommand(questionId, request.Text, request.AssetId, request.IsCorrect, request.SortOrder), ct);
            if (result.IsFailed)
                return this.ToProblem(result); //404 if the question doesn't exist, 422 if validation fails

            var response = AnswerResponse.From(result.Value);
            //There's no "GET /answers/{id}" endpoint, so the Location header points at the question the answer belongs to
            return Created($"/api/v1/questions/{questionId}", response);
        }

        [HttpPut("answers/{answerId:guid}")] //Update an answer (text, picture, correct or not, order)
        public async Task<ActionResult<AnswerResponse>> Update(Guid answerId, [FromBody] AnswerRequest request, CancellationToken ct)
        {
            var result = await updateAnswerUseCase.UpdateAsync(
                new UpdateAnswerCommand(answerId, request.Text, request.AssetId, request.IsCorrect, request.SortOrder), ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(AnswerResponse.From(result.Value));
        }

        [HttpDelete("answers/{answerId:guid}")] //Soft delete, so the statistics survive
        public async Task<IActionResult> Delete(Guid answerId, CancellationToken ct)
        {
            var result = await deleteAnswerUseCase.DeleteAsync(new DeleteAnswerCommand(answerId), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }
    }
}
