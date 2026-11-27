using System.Diagnostics;
using Application.Commands.StaffCommands;
using Application.ErrorMessages;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Domain.Services;
using FluentResults;

namespace Application.UseCases.Staff
{
    // Sets a new password. Changing your own needs the current password; an admin resetting someone else's doesn't.
    // Afterwards every login of that staff member is revoked (all devices, also the one that made the change),
    // so a stolen or shared login can't keep refreshing with the old session.
    public class SetStaffPasswordUseCase : ISetStaffPasswordUseCase
    {
        private readonly IGetByIdAsync getById;
        private readonly IUpdateAsync update;
        private readonly IHash hasher;
        private readonly IVerify verifyPassword;
        private readonly IRevokeAllLoginsForStaffAsync revokeAllLogins;
        private readonly TimeProvider time;
        private readonly IPasswordValidatorService _passwordValidator;
        public SetStaffPasswordUseCase(IGetByIdAsync getById, IUpdateAsync update, IHash hasher, IVerify verifyPassword,
                                       IRevokeAllLoginsForStaffAsync revokeAllLogins, TimeProvider time)
        {
            this.getById = getById;
            this.update = update;
            this.hasher = hasher;
            this.verifyPassword = verifyPassword;
            this.revokeAllLogins = revokeAllLogins;
            this.time = time;
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

                // The spec: all existing logins end. Their access tokens still work for at most 15 minutes (stateless JWT),
                // but they can no longer be refreshed.
                await revokeAllLogins.RevokeAllLoginsForStaffAsync(member.Id, time.GetUtcNow(), ct);
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
