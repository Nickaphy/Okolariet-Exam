using Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.InBoundPort;
using Application.InBoundPort.ISessionUseCases;
using Application.Commands.Sessions;
using Api.Responses.SessionsResponses;
using Api.Requests.SessionRequests;

namespace Api.Controllers
{
    [ApiController] 
    [AllowAnonymous]  // Allows users to access the session endpoint without authentication.
    [Route("api/v1/sessions")] // Route for session-related actions.
    public class SessionController (IPostSessionAsync postSessionUsecase, IGetSessionUseCase getSessionUseCase) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<SessionResponse>> Create(
            [FromBody] SessionRequest request,
            CancellationToken ct)
        {
            var command = new CreateSessionCommand(request.ProjectId); // Creates the application layer command record  
            var result = await postSessionUsecase.PostSessionAsync(command, ct); // Calls the inbound port for the application

            if (result.IsFailed)
                return this.ToProblem(result);

            var session = result.Value;
            var response = new SessionResponse(session.Id, session.ProjectId, session.StartedAt);

            return Created($"/api/v1/sessions/{response.Id}", response); 
        }

    // GET /api/v1/sessions/{sessionId}
    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<SessionSummaryResponse>> Get(
        Guid sessionId,
        CancellationToken ct)
    {
        // The session ID comes from the URL.
        var result = await getSessionUseCase.GetAsync(sessionId, ct);

        // A missing session becomes HTTP 404.
        if (result.IsFailed)
            return this.ToProblem(result);

        // Copy the Application summary into the API response.
        var summary = result.Value;
        var response = new SessionSummaryResponse(
            summary.Id,
            summary.StartedAt,
            summary.TotalQuestions,
            summary.Answered,
            summary.Correct,
            summary.Percentage,
            summary.AllGuestsAveragePercentage);

        return Ok(response);
    }
    }
}