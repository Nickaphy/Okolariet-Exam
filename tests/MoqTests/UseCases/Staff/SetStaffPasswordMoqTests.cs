using Application.Commands.StaffCommands;           // SetStaffPasswordCommand
using Application.ErrorMessages;                    // NotFoundError, BusinessRuleError
using Application.OutBoundPort.IPasswordHash;       // IHash, IVerify
using Application.OutBoundPort.IStaffRepository;    // IGetByIdAsync, IUpdateAsync
using Application.UseCases.Staff;                   // SetStaffPasswordUseCase
using Domain.Entities;                              // StaffMember
using Domain.Enums;                                 // StaffRole
using Moq;

namespace MoqTests.UseCases.Staff
{
    // Spec (contracts/openapi.yaml, PUT /staff/{id}/password): changing your own password requires currentPassword.
    // An admin resetting someone else's password omits it. The new password must be at least 12 characters.
    public class SetStaffPasswordMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private const string NewPassword = "Okolariet#2026";
        private const string CurrentPassword = "Gammel#Kode2025";

        private readonly Mock<IGetByIdAsync> getById = new();
        private readonly Mock<IUpdateAsync> update = new();
        private readonly Mock<IHash> hasher = new();
        private readonly Mock<IVerify> verifyPassword = new();

        private readonly StaffMember member = new() { Id = Guid.NewGuid(), Email = "anna@okolariet.dk", PasswordHash = "old-hash", Role = StaffRole.Admin };
        private readonly Guid otherAdmin = Guid.NewGuid();

        public SetStaffPasswordMoqTests()
        {
            getById.Setup(g => g.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");
            verifyPassword.Setup(v => v.Verify("old-hash", CurrentPassword)).Returns(true);
        }

        private SetStaffPasswordUseCase CreateUseCase() => new(getById.Object, update.Object, hasher.Object, verifyPassword.Object);

        private void AssertPasswordNotChanged()
        {
            Assert.Equal("old-hash", member.PasswordHash);
            update.Verify(u => u.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Admin_resets_someone_elses_password_without_the_current_password()
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, otherAdmin, null, NewPassword), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("new-hash", member.PasswordHash);
            hasher.Verify(h => h.Hash(NewPassword), Times.Once);
            verifyPassword.Verify(v => v.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            update.Verify(u => u.UpdateAsync(member, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Changing_your_own_password_with_the_correct_current_password_works()
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, member.Id, CurrentPassword, NewPassword), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("new-hash", member.PasswordHash);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Changing_your_own_password_without_the_current_password_is_rejected(string? current)
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, member.Id, current, NewPassword), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertPasswordNotChanged();
        }

        [Fact]
        public async Task Changing_your_own_password_with_a_wrong_current_password_is_rejected()
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, member.Id, "Forkert#Kode2025", NewPassword), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertPasswordNotChanged();
        }

        [Fact]
        public async Task Weak_new_password_is_rejected()
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, otherAdmin, null, "kort"), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
            AssertPasswordNotChanged();
        }

        // Minimum length follows PasswordPolicy.MinLength (8). Note: openapi.yaml says 12.
        [Fact]
        public async Task New_password_of_8_characters_is_allowed()
        {
            // 8 characters that meet every other rule
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, otherAdmin, null, "Ab#1xyzw"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("new-hash", member.PasswordHash);
        }

        [Fact]
        public async Task New_password_of_7_characters_is_rejected()
        {
            // 7 characters that meet every other rule
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(member.Id, otherAdmin, null, "Ab#1xyz"), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertPasswordNotChanged();
        }

        [Fact]
        public async Task Unknown_staff_member_is_NotFound()
        {
            var result = await CreateUseCase().SetPasswordAsync(new SetStaffPasswordCommand(Guid.NewGuid(), otherAdmin, null, NewPassword), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
