using System.Diagnostics;
using Application.Commands.StaffCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IStaffRepository;
using Domain.Enums;
using FluentResults;

namespace Application.UseCases.Staff
{
    // Delete a Staff member, however the system requires that their is atleast one admin in the system at all times
    public class DeleteStaffUseCase: IDeleteStaffUseCase
    {
        private readonly IGetByIdAsync getById;
        private readonly IDeleteAsync delete;
        private readonly ICountAdminsAsync countAdmins;
        private static readonly SemaphoreSlim _Lock = new(1, 1); // Lock to prevent a double click or race condition from deleting the last admin.
        public DeleteStaffUseCase(IGetByIdAsync getById, IDeleteAsync delete, ICountAdminsAsync countAdmins)
        {
            this.getById = getById;
            this.delete = delete;
            this.countAdmins = countAdmins;
        }
        public async Task<Result> DeleteAsync(DeleteStaffCommand cmd, CancellationToken ct)
        {
            await _Lock.WaitAsync();
            try
            {
                if (cmd.Id == cmd.RequestedById)
                    return Result.Fail(new BusinessRuleError("You cannot delete yourself."));   // 422

                var member = await getById.GetByIdAsync(cmd.Id, ct);
                if (member is null) 
                    return Result.Fail(new NotFoundError("Staff member not found.")); // User input error -> we could consider changing to 404, but it feels like a business rule error.

                if (member.Role == StaffRole.Admin && await countAdmins.CountAdminsAsync(ct) <= 1)
                    return Result.Fail(new BusinessRuleError("You cannot delete the last admin."));   // 422

                await delete.DeleteAsync(member, ct);
                return Result.Ok();
            }
            catch (Exception ex) // Don't expose EX to the user, only log it.
            {
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail(new UnexpectedError($"An unexpected error occurred while deleting staff member"));
            }
            finally // Ensure we always release lock
            {
                _Lock.Release();
            }
        }
    }
}
