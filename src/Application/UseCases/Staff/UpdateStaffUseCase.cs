using System.Diagnostics;
using Application.Commands.StaffCommands;
using Application.ErrorMessages;
using Application.Helpers;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

namespace Application.UseCases.Staff
{
    // Let's an Admin update a staff memmber's email and role. The system must always have at least one admin.
    public class UpdateStaffUseCase : IUpdateStaffUseCase
    {
        private readonly IGetByIdAsync getById;
        private readonly IEmailExistsAsync emailExists;
        private readonly ICountAdminsAsync countAdmins;
        private readonly IUpdateAsync update;
        public UpdateStaffUseCase(IGetByIdAsync getById, IEmailExistsAsync emailExists, ICountAdminsAsync countAdmins, IUpdateAsync update)
        {
            this.getById = getById;
            this.emailExists = emailExists;
            this.countAdmins = countAdmins;
            this.update = update;
        }
        public async Task<Result<StaffMember>> UpdateAsync(UpdateStaffCommand cmd, CancellationToken ct)
        {
            try
            {
                var member = await getById.GetByIdAsync(cmd.Id, ct);
                if (member is null) return Result.Fail(new NotFoundError("Staff member not found."));

                var email = EmailNormalizer.Normalize(cmd.Email);
                if (email != member.Email && await emailExists.EmailExistsAsync(email, member.Id, ct))
                    return Result.Fail(new ConflictError("The e-mail is already in use."));   // 409

                if (member.Role == StaffRole.Admin && cmd.Role != StaffRole.Admin
                    && await countAdmins.CountAdminsAsync(ct) <= 1)
                    return Result.Fail(new BusinessRuleError("The system must keep at least one admin."));   // 422

                member.Email = email;
                member.Role = cmd.Role;
                await update.UpdateAsync(member, ct);
                return Result.Ok(member);
            }
            catch (Exception ex) // We do not return the exception to the user, as it can contain sensitive information. Instead we log it and return a generic error message.
            {
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail(new UnexpectedError("An unexpected error occurred while updating the staff member."));
            }
        }
    }
}
