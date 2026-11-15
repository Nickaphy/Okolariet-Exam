using Application.Commands.StaffCommands;
using Application.ErrorMessages;
using Application.Helpers;
using Application.InBoundPort.IStaffUseCases;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Domain.Services;
using FluentResults;

namespace Application.UseCases.Staff
{
    public class CreateStaffUseCase : ICreateStaffUseCase
    {
        private readonly IAddAsync add;
        private readonly IEmailExistsAsync emailExists;
        private readonly IHash hasher;
        private readonly IPasswordValidatorService _passwordValidator;
        private readonly TimeProvider time; // Use TimeProvider to give a consistent time source for testing and production.
        public CreateStaffUseCase(IAddAsync add, IEmailExistsAsync emailExists, IHash hasher, TimeProvider time)
        {
            this.add = add;
            this.emailExists = emailExists;
            this.hasher = hasher;
            this.time = time;
            this._passwordValidator = new PasswordValidatorService();
        }
        public async Task<Result<StaffMember>> CreateAsync(CreateStaffCommand cmd, CancellationToken ct)
        {
            var email = EmailNormalizer.Normalize(cmd.Email);

            var passwordCheck = _passwordValidator.Validate(cmd.Password, email, name: null);
            if (!passwordCheck.IsValid)
                return Result.Fail(new BusinessRuleError(string.Join(" ", passwordCheck.Errors)));

            if (await emailExists.EmailExistsAsync(email, null, ct))
                return Result.Fail(new ConflictError("The e-mail is already in use."));   // 409

            var member = new StaffMember
            {
                Id = Guid.CreateVersion7(),
                Email = email,
                PasswordHash = hasher.Hash(cmd.Password),
                Role = cmd.Role,
                CreatedAt = time.GetUtcNow()
            };
            await add.AddAsync(member, ct);
            return Result.Ok(member);
        }
    }
}
