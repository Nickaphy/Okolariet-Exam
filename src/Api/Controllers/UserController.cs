using Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.InBoundPort.IUserUsecases;
using Application.Commands.UserCommands;
using Api.Responses.UserResponse;
using Api.Requests.UserRequests;
using Api.Responses;

namespace Api.Controllers
{
[ApiController]
[Route("api/v1/users")]
public class UserController(ICreateUserUseCase createUserUseCase) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        // Create the application layer command record from the gathered request data
        var command = new CreateUserCommand(
            request.SessionId,
            request.ModdedTemplateId,
            request.Nickname.FirstWordId,
            request.Nickname.SecondWordId,
            request.Nickname.Number);

        var result = await createUserUseCase.PostUserAsync(command, ct); // Calls the inbound port for the application

        // If the result is failed, return a problem response with the error details
        if (result.IsFailed)
            return this.ToProblem(result);

        // If the result is successful, create a response object and return it with a Created status
        var user = result.Value;
        var response = new UserResponse(
            user.Id,
            user.NickName,
            user.ModdedTemplateId,
            new ScoreResponse(
                user.Score.Correct,
                user.Score.TotalQuestions,
                user.Score.Percentage));

        return Created($"/api/v1/users/{response.Id}", response);
    }
}
}