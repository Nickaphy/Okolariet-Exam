using Application.Commands.NicknameCommands;            // AddNicknameWordCommand
using Application.ErrorMessages;                        // ConflictError
using Application.OutBoundPort.INicknameWordRepository; // IExistsAsync, IAddAsync
using Application.UseCases.Nickname;                    // AddWordUseCase
using Domain.Entities;                                  // NicknameWord
using Moq;

namespace MoqTests.UseCases
{
    public class NicknameWordMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IExistsAsync> exists = new();
        private readonly Mock<IAddAsync> add = new();

        private AddWordUseCase CreateUseCase() => new(exists.Object, add.Object);

        [Fact]
        public async Task Add_new_word_calls_AddAsync_once()
        {
            // Arrange: the word does not exist yet
            exists.Setup(e => e.ExistsAsync("Modig", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

            // Act
            var result = await CreateUseCase().AddWordAsync(new AddNicknameWordCommand("Modig"), Ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Modig", result.Value.Text);
            add.Verify(a => a.AddAsync(
                    It.Is<NicknameWord>(w => w.Text == "Modig" && w.IsActive),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Add_existing_word_is_Conflict_and_never_calls_AddAsync()
        {
            exists.Setup(e => e.ExistsAsync("Modig", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

            var result = await CreateUseCase().AddWordAsync(new AddNicknameWordCommand("Modig"), Ct);

            Assert.True(result.HasError<ConflictError>());
            add.Verify(a => a.AddAsync(It.IsAny<NicknameWord>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}