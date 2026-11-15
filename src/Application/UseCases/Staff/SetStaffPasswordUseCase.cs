using System.Diagnostics;
using Application.Commands.StaffCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Domain.Services;
using FluentResults;

namespace Application.UseCases.Staff
{
    public class SetStaffPasswordUseCase : ISetStaffPasswordUseCase
    {
        private readonly IGetByIdAsync getById;
        private readonly IUpdateAsync update;
        private readonly IHash hasher;
        private readonly IVerify verifyPassword;
        private readonly IPasswordValidatorService _passwordValidator;
        public SetStaffPasswordUseCase(IGetByIdAsync getById, IUpdateAsync update, IHash hasher, IVerify verifyPassword)
        {
            this.getById = getById;
            this.update = update;
            this.hasher = hasher;
            this.verifyPassword = verifyPassword;
            this._passwordValidator = new PasswordValidatorService();
        }
        public async Task<Result> SetPasswordAsync(SetStaffPasswordCommand cmd, CancellationToken ct)
        {
            try
            {
                var member = await getById.GetByIdAsync(cmd.Id, ct);
                if (member is null) 
                    return Result.Fail(new NotFoundError("Staff member not found."));

                var passwordCheck = _passwordValidator.Validate(cmd.NewPassword, member.Email, name: null);
                if (!passwordCheck.IsValid)
                    return Result.Fail(new BusinessRuleError(string.Join(" ", passwordCheck.Errors)));

                var changingOwn = cmd.Id == cmd.RequestedById;
                if (changingOwn && (string.IsNullOrEmpty(cmd.CurrentPassword) || !verifyPassword.Verify(member.PasswordHash, cmd.CurrentPassword)))
                    return Result.Fail(new BusinessRuleError("The current password is missing or wrong."));   // 422

                member.PasswordHash = hasher.Hash(cmd.NewPassword);
                await update.UpdateAsync(member, ct);

                // TODO (Auth slice): revoke all logins of this staff member, as the spec requires.
                return Result.Ok();
            }
            catch(Exception ex) //Only log EX
            {
                Debug.WriteLine($"Infrastructure Failure: {ex.Message}");
                return Result.Fail(new UnexpectedError($"An unexpected error occurred while setting the staff member's password"));
            }
        }
    }
}
