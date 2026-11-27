using Application.ErrorMessages;                    // BusinessRuleError
using Application.Helpers;                          // PagedList
using Application.OutBoundPort.IUserRepository;     // IListUsersAsync
using Application.UseCases.Users;                   // ListUsersUseCase
using Domain.Entities;                              // User
using Moq;

namespace MoqTests.UseCases.Users
{
    // The leaderboard is public, so the use case checks paging and sort itself instead of trusting the controller.
    public class ListUsersMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IListUsersAsync> listUsers = new();

        private ListUsersUseCase CreateUseCase() => new(listUsers.Object);

        private void AssertRepositoryNotCalled()
            => listUsers.Verify(l => l.ListAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

        [Theory]
        [InlineData("bestScore", 1, 1)]
        [InlineData("newest", 2, 100)]
        public async Task Valid_request_is_passed_on_unchanged(string sort, int page, int pageSize)
        {
            var quizId = Guid.NewGuid();
            var entries = new PagedList<User>(Array.Empty<User>(), page, pageSize, 0);
            listUsers.Setup(l => l.ListAsync(quizId, sort, page, pageSize, It.IsAny<CancellationToken>())).ReturnsAsync(entries);

            var result = await CreateUseCase().ListAsync(quizId, sort, page, pageSize, Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(entries, result.Value);
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task Invalid_paging_is_BusinessRuleError(int page, int pageSize)
        {
            var result = await CreateUseCase().ListAsync(null, "bestScore", page, pageSize, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertRepositoryNotCalled();
        }

        [Fact]
        public async Task Unknown_sort_is_BusinessRuleError()
        {
            var result = await CreateUseCase().ListAsync(null, "oldest", 1, 20, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertRepositoryNotCalled();
        }
    }
}
