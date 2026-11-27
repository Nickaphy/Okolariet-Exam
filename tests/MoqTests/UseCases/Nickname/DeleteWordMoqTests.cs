using Application.Commands.NicknameCommands;                // DeleteNicknameWordCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.OutBoundPort.INicknameWordRepository;     // IGetAsync, IRemoveAsync, ICountActiveAsync
using Application.UseCases.Nickname;                        // DeleteWordUseCase
using Domain.Entities;                                      // NicknameWord
using Moq;

namespace MoqTests.UseCases.Nickname
{
    // Spec (contracts/openapi.yaml, DELETE /nickname-words/{id}):
    // blocked with 422 if fewer than 10 words would remain, since the kiosk needs 10 to make suggestions.
    public class DeleteWordMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetAsync> get = new();
        private readonly Mock<IRemoveAsync> remove = new();
        private readonly Mock<ICountActiveAsync> countActive = new();

        private readonly NicknameWord word = NicknameWord.Create("modig");

        public DeleteWordMoqTests()
        {
            get.Setup(g => g.GetAsync(word.Id, It.IsAny<CancellationToken>())).ReturnsAsync(word);
        }

        private DeleteWordUseCase CreateUseCase() => new(get.Object, remove.Object, countActive.Object);

        [Fact]
        public async Task Deleting_when_11_words_exist_leaves_10_and_is_allowed()
        {
            countActive.Setup(c => c.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(11);

            var result = await CreateUseCase().DeleteWordAsync(new DeleteNicknameWordCommand(word.Id), Ct);

            Assert.True(result.IsSuccess);
            remove.Verify(r => r.RemoveAsync(word, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deleting_when_10_words_exist_would_leave_9_and_is_blocked()
        {
            countActive.Setup(c => c.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10);

            var result = await CreateUseCase().DeleteWordAsync(new DeleteNicknameWordCommand(word.Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            remove.Verify(r => r.RemoveAsync(It.IsAny<NicknameWord>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Unknown_word_is_NotFound()
        {
            var result = await CreateUseCase().DeleteWordAsync(new DeleteNicknameWordCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            remove.Verify(r => r.RemoveAsync(It.IsAny<NicknameWord>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
