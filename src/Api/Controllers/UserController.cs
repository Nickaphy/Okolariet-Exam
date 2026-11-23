using Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.InBoundPort.IUserUsecases;
using Application.Commands.UserCommands;
using Api.Responses.UserResponse;
using Api.Requests.UserRequests;
using Api.Responses;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Api.Controllers
{
[ApiController]
[Route("api/v1/users")]
public class UserController(ICreateUserUseCase createUserUseCase, IGetUserUseCase getUserUseCase, IDeleteUserUseCase deleteUserUseCase, IClearQuizLeaderboardUseCase clearQuizLeaderboardUseCase, IListUsersUseCase listUsersUseCase) : ControllerBase
{
    // Create the User
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

    // GET /api/v1/users/{userId}
    [AllowAnonymous]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> Get(
        Guid userId,
        CancellationToken ct)
    {
        var result = await getUserUseCase.GetAsync(userId, ct);

        if (result.IsFailed)
            return this.ToProblem(result);

        var user = result.Value;

        // If the result is successful, create a response object and return it with a Created status
        return Ok(new UserResponse(
            user.Id,
            user.NickName,
            user.ModdedTemplateId,
            new ScoreResponse(
                user.Score.Correct,
                user.Score.TotalQuestions,
                user.Score.Percentage)));
    }
   
   // DELETE /api/v1/users/{userId}
   [Authorize(Roles = "Admin")] // Only admins can delete an entry
   [HttpDelete("{userId:guid}")]
   public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        // await the deletion 
        var result = await deleteUserUseCase.DeleteAsync(userId, ct);

        if (result.IsFailed)
            return this.ToProblem(result);

        return NoContent();
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("~/api/v1/modded-templates/{moddedTemplateId:guid}/users")]
    public async Task<IActionResult> ClearLeaderboard(
    Guid moddedTemplateId,
    CancellationToken ct)
    {
        var result = await clearQuizLeaderboardUseCase.ClearAsync(
            moddedTemplateId, ct);

        if (result.IsFailed)
            return this.ToProblem(result);

        return NoContent();
    }

    // GET /api/v1/users
    // Returns a page of saved leaderboard entries.
    // Example: /api/v1/users?sort=bestScore&page=1&pageSize=20
    [AllowAnonymous] // Guests can view the leaderboard without logging in.
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> List(
        // Guid? allows null. Omitting this filter includes entries from all quizzes.
        [FromQuery] Guid? moddedTemplateId = null,

        // Only these two sorting options are accepted. Default is bestScore.
        [FromQuery, RegularExpression("^(bestScore|newest)$")]
        string sort = "bestScore",

        // Page numbers start at 1. Each page contains up to pageSize entries.
        [FromQuery, Range(1, int.MaxValue)] int page = 1,

        // Limit each page to 1-100 entries. Default is 20.
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default)
    {
        // ASP.NET reads the parameters from the URL's query string.
        // With [ApiController], invalid values caught by the attributes above
        // produce HTTP 400 before this method's body runs.

        // Call the Application layer through its inbound port.
        // Pass the optional quiz filter, sorting option, and pagination settings.
        var result = await listUsersUseCase.ListAsync(
            moddedTemplateId, sort, page, pageSize, ct);

        // Translate an Application failure into an HTTP problem response.
        if (result.IsFailed)
            return this.ToProblem(result);

        // Unwrap the successful Result to access the PagedList<User>.
        // It contains the domain entries and the pagination information.
        var entries = result.Value;

        // Select converts each User entity into a UserResponse.
        // The nested ScoreResponse carries the correct count, total, and percentage.
        // ToList executes this mapping and collects the response objects.
        var items = entries.Items.Select(user => new UserResponse(
            user.Id,
            user.NickName,
            user.ModdedTemplateId,
            new ScoreResponse(
                user.Score.Correct,
                user.Score.TotalQuestions,
                user.Score.Percentage)))
            .ToList();

        // Return HTTP 200 with the entries and pagination information as JSON.
        // TotalCount counts ALL matching entries before pagination.
        // For example, Items might contain 20 entries while TotalCount is 135.
        return Ok(new PagedResponse<UserResponse>(
            items,
            entries.Page,
            entries.PageSize,
            entries.TotalCount));
    }
}
}