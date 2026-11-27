using Application.Commands.StaffCommands;           // UpdateStaffCommand
using Application.ErrorMessages;                    // NotFoundError, ConflictError, UnexpectedError
using Application.OutBoundPort.IStaffRepository;    // IGetByIdAsync, IEmailExistsAsync, ICountAdminsAsync, IUpdateAsync
using Application.UseCases.Staff;                   // UpdateStaffUseCase
using Domain.Entities;                              // StaffMember
using Domain.Enums;                                 // StaffRole
using Moq;

namespace MoqTests.UseCases.Staff
{
    // Spec (contracts/openapi.yaml, PUT /staff/{id}): 409 if the e-mail is taken,
    // 422 if the change would leave the system without an admin.
    // (StaffRole only has Admin today, so "lose the admin role" can't be tested yet.)
    public class UpdateStaffMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetByIdAsync> getById = new();
        private readonly Mock<IEmailExistsAsync> emailExists = new();
        private readonly Mock<ICountAdminsAsync> countAdmins = new();
        private readonly Mock<IUpdateAsync> update = new();

        private readonly StaffMember member = new() { Id = Guid.NewGuid(), Email = "anna@okolariet.dk", PasswordHash = "hash", Role = StaffRole.Admin };

        public UpdateStaffMoqTests()
        {
            getById.Setup(g => g.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        }

        private UpdateStaffUseCase CreateUseCase() => new(getById.Object, emailExists.Object, countAdmins.Object, update.Object);

        [Fact]
        public async Task Update_saves_the_new_normalized_email()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateStaffCommand(member.Id, " New@Okolariet.DK ", StaffRole.Admin), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("new@okolariet.dk", member.Email);
            emailExists.Verify(e => e.EmailExistsAsync("new@okolariet.dk", member.Id, It.IsAny<CancellationToken>()), Times.Once);
            update.Verify(u => u.UpdateAsync(member, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Email_used_by_someone_else_is_Conflict_and_nothing_changes()
        {
            emailExists.Setup(e => e.EmailExistsAsync("taken@okolariet.dk", member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().UpdateAsync(new UpdateStaffCommand(member.Id, "taken@okolariet.dk", StaffRole.Admin), Ct);

            Assert.True(result.HasError<ConflictError>());
            Assert.Equal("anna@okolariet.dk", member.Email);
            update.Verify(u => u.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Own_email_with_other_casing_is_not_a_conflict()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateStaffCommand(member.Id, "ANNA@okolariet.dk", StaffRole.Admin), Ct);

            Assert.True(result.IsSuccess);
            emailExists.Verify(e => e.EmailExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_staff_member_is_NotFound()
        {
            var result = await CreateUseCase().UpdateAsync(new UpdateStaffCommand(Guid.NewGuid(), "new@okolariet.dk", StaffRole.Admin), Ct);

            Assert.True(result.HasError<NotFoundError>());
            update.Verify(u => u.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Database_error_becomes_UnexpectedError_without_leaking_details()
        {
            update.Setup(u => u.UpdateAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("Server=db;Password=secret"));

            var result = await CreateUseCase().UpdateAsync(new UpdateStaffCommand(member.Id, "new@okolariet.dk", StaffRole.Admin), Ct);

            Assert.True(result.HasError<UnexpectedError>());
            Assert.DoesNotContain(result.Errors, e => e.Message.Contains("secret"));
        }
    }
}
