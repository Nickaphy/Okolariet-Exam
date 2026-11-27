using System.ComponentModel.DataAnnotations;
using Api.Helpers;
using Api.Requests.StaffRequests;
using Api.Responses.StaffResponses;
using Application.Commands.StaffCommands;
using Application.InBoundPort.IStaffUseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // This is a general controller for managing staff members. It provides endpoints for listing, creating, retrieving, updating, deleting staff members, and setting their passwords.
    // All endpoints are secured and require the user to have the "Admin" role.
    [ApiController]
    [Route("api/v1/staff")]
    public class StaffController(IListStaffUseCase listStaffUseCase, 
                                ICreateStaffUseCase createStaffUseCase,
                                IGetStaffUseCase getStaffUseCase,
                                IUpdateStaffUseCase updateStaffUseCase, 
                                IDeleteStaffUseCase deleteStaffUseCase, 
                                ISetStaffPasswordUseCase setStaffPasswordUseCase) : ControllerBase
    {
        [HttpGet] // All staff members, paginated. Admin only.
        public async Task<ActionResult<PagedResponse<StaffResponse>>> List(
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await listStaffUseCase.ListAsync(page, pageSize, ct);
            if (result.IsFailed) 
                return this.ToProblem(result); // Uses .ToProblem extension method to convert FluentResults errors into HTTP responses.

            var p = result.Value;
            return Ok(new PagedResponse<StaffResponse>(
                p.Items.Select(StaffResponse.From).ToList(), p.Page, p.PageSize, p.TotalCount));
        }

        [HttpPost]
        public async Task<ActionResult<StaffResponse>> Create([FromBody] CreateStaffRequest request, CancellationToken ct)
        {
            var result = await createStaffUseCase.CreateAsync(new CreateStaffCommand(request.Email, request.Password, request.Role), ct);
            if (result.IsFailed) 
                return this.ToProblem(result);

            var response = StaffResponse.From(result.Value);
            return CreatedAtAction(nameof(Get), new { staffId = response.Id }, response); // CreatedAtAction returns a 201 (201 = Created) Created response with a Location header pointing to the newly created resource.
        }

        [HttpGet("me")] // Get the currently logged-in staff member's information
        public async Task<ActionResult<StaffResponse>> GetCurrent(CancellationToken ct) 
        {
            var result = await getStaffUseCase.GetAsync(User.GetStaffId(), ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(StaffResponse.From(result.Value));
        }

        [HttpGet("{staffId:guid}")] //Get a specific staff member's information by their ID
        public async Task<ActionResult<StaffResponse>> Get(Guid staffId, CancellationToken ct)
        {
            var result = await getStaffUseCase.GetAsync(staffId, ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(StaffResponse.From(result.Value));
        }

        [HttpPut("{staffId:guid}")]
        public async Task<ActionResult<StaffResponse>> Update(Guid staffId, [FromBody] UpdateStaffRequest request, CancellationToken ct)
        {
            var result = await updateStaffUseCase.UpdateAsync(new UpdateStaffCommand(staffId, request.Email, request.Role), ct);
            return result.IsFailed ? this.ToProblem(result) : Ok(StaffResponse.From(result.Value));
        }

        [HttpDelete("{staffId:guid}")]
        public async Task<IActionResult> Delete(Guid staffId, CancellationToken ct)
        {
            var result = await deleteStaffUseCase.DeleteAsync(new DeleteStaffCommand(staffId, User.GetStaffId()), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }

        [HttpPut("{staffId:guid}/password")] // Endpoint for changing a staff member's password. Requires the current password for verification. May need changes in the future to allow admins to reset passwords without knowing the current password.
        public async Task<IActionResult> SetPassword(Guid staffId, [FromBody] SetStaffPasswordRequest request, CancellationToken ct)
        {
            var result = await setStaffPasswordUseCase.SetPasswordAsync(
                new SetStaffPasswordCommand(staffId, User.GetStaffId(), request.CurrentPassword, request.NewPassword), ct);
            return result.IsFailed ? this.ToProblem(result) : NoContent();
        }
    }
}
