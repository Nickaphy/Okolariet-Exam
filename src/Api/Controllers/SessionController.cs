using System.ComponentModel.DataAnnotations;
using Api.Helpers;
using Api.Requests.NicknameRequests;
using Api.Responses.NicknameResponse;
using Application.Commands.NicknameCommands;
using Application.InBoundPort.INicknameUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.InBoundPort;
using Application.Commands.Sessions;
using Api.Responses.SessionsResponses;
using Api.Requests.SessionRequests;

namespace Api.Controllers
{
    [ApiController] 
    [AllowAnonymous]  // Allows users to access the session endpoint without authentication.
    [Route("api/v1/sessions")] // Route for session-related actions.
    public class SessionController (IPostSessionAsync postSessionUsecase) : ControllerBase
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

    }
}