using Application.Commands.StaffCommands;           // DeleteStaffCommand
using Application.ErrorMessages;                    // NotFoundError, BusinessRuleError, UnexpectedError
using Application.OutBoundPort.IStaffRepository;    // IGetByIdAsync, IDeleteAsync, ICountAdminsAsync
using Application.UseCases.Staff;                   // DeleteStaffUseCase
using Domain.Entities;                              // StaffMember
using Domain.Enums;                                 // StaffRole
using Moq;

namespace MoqTests.UseCases.Staff
{
    // Spec (contracts/openapi.yaml, DELETE /staff/{id}): blocked with 422 if it would delete yourself or the last admin.
    public class DeleteStaffMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetByIdAsync> getById = new();
        private readonly Mock<IDeleteAsync> delete = new();
        private readonly Mock<ICountAdminsAsync> countAdmins = new();

        private readonly StaffMember member = new() { Id = Guid.NewGuid(), Email = "anna@okolariet.dk", Role = StaffRole.Admin };
        private readonly Guid requestedBy = Guid.NewGuid();   // another admin

        public DeleteStaffMoqTests()
        {
            getById.Setup(g => g.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        }

        private DeleteStaffUseCase CreateUseCase() => new(getById.Object, delete.Object, countAdmins.Object);

        [Fact]
        public async Task Deleting_an_admin_works_when_there_are_two()
        {
            countAdmins.Setup(c => c.CountAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var result = await CreateUseCase().DeleteAsync(new DeleteStaffCommand(member.Id, requestedBy), Ct);

            Assert.True(result.IsSuccess);
            delete.Verify(d => d.DeleteAsync(member, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleting_yourself_is_blocked()
        {
            countAdmins.Setup(c => c.CountAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var result = await CreateUseCase().DeleteAsync(new DeleteStaffCommand(member.Id, member.Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Contains(result.Errors, e => e.Message == "You cannot delete yourself.");
            delete.Verify(d => d.DeleteAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Deleting_the_last_admin_is_blocked()
        {
            countAdmins.Setup(c => c.CountAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await CreateUseCase().DeleteAsync(new DeleteStaffCommand(member.Id, requestedBy), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Contains(result.Errors, e => e.Message == "You cannot delete the last admin.");
            delete.Verify(d => d.DeleteAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_staff_member_is_NotFound()
        {
            var result = await CreateUseCase().DeleteAsync(new DeleteStaffCommand(Guid.NewGuid(), requestedBy), Ct);

            Assert.True(result.HasError<NotFoundError>());
            delete.Verify(d => d.DeleteAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Database_error_is_UnexpectedError_and_the_lock_is_released()
        {
            countAdmins.Setup(c => c.CountAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
            delete.SetupSequence(d => d.DeleteAsync(member, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("database is down"))
                  .Returns(Task.CompletedTask);
            var useCase = CreateUseCase();

            var first = await useCase.DeleteAsync(new DeleteStaffCommand(member.Id, requestedBy), Ct);
            // If the lock was not released, this call would wait forever. WaitAsync makes it fail after 5 seconds instead.
            var second = await useCase.DeleteAsync(new DeleteStaffCommand(member.Id, requestedBy), Ct).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(first.HasError<UnexpectedError>());
            Assert.DoesNotContain(first.Errors, e => e.Message.Contains("database is down"));
            Assert.True(second.IsSuccess);
        }
    }
}
