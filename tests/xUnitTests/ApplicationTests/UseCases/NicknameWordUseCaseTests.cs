using Application.Commands.NicknameCommands;   // AddNicknameWordCommand, DeleteNicknameWordCommand
using Application.ErrorMessages;               // ConflictError, NotFoundError, BusinessRuleError
using Application.UseCases.Nickname;           // AddWordUseCase, DeleteWordUseCase
using Domain.Entities;                         // NicknameWord
using xUnitTests.ApplicationTests.Fakes;       // FakeNicknameWordRepository

namespace xUnitTests.ApplicationTests.UseCases
{
    public class NicknameWordUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeNicknameWordRepository words = new();

        // Puts n active words in the fake list.
        private void AddWords(int count)
        {
            for (var i = 0; i < count; i++)
                words.Words.Add(NicknameWord.Create($"Ord{i}"));
        }

        // ---------- Add ----------

        [Fact]
        public async Task Add_saves_a_new_active_word()
        {
            var useCase = new AddWordUseCase(words, words);

            var result = await useCase.AddWordAsync(new AddNicknameWordCommand("Modig"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal("Modig", result.Value.Text);
            Assert.True(result.Value.IsActive);
            Assert.Same(result.Value, Assert.Single(words.Words));
        }

        [Fact]
        public async Task Add_existing_word_is_Conflict_and_saves_nothing()
        {
            words.Words.Add(NicknameWord.Create("Modig"));
            var useCase = new AddWordUseCase(words, words);

            var result = await useCase.AddWordAsync(new AddNicknameWordCommand("Modig"), Ct);

            Assert.True(result.HasError<ConflictError>());
            Assert.Single(words.Words);
        }

        [Fact]
        public async Task Add_same_word_with_other_casing_is_Conflict()
        {
            // The use case trusts IExistsAsync to compare case-insensitively (spec: unique, case-insensitive).
            words.Words.Add(NicknameWord.Create("Modig"));
            var useCase = new AddWordUseCase(words, words);

            var result = await useCase.AddWordAsync(new AddNicknameWordCommand("MODIG"), Ct);

            Assert.True(result.HasError<ConflictError>());
        }

        [Fact]
        public async Task Add_blank_word_throws_instead_of_returning_a_failed_result()
        {
            // Documents current behaviour: the entity throws and the use case doesn't catch it.
            // The Api's [Required] attribute stops blank words before they get here.
            var useCase = new AddWordUseCase(words, words);

            await Assert.ThrowsAsync<ArgumentException>(() => useCase.AddWordAsync(new AddNicknameWordCommand(" "), Ct));
            Assert.Empty(words.Words);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_removes_the_word_when_more_than_10_are_left()
        {
            AddWords(11);
            var target = words.Words[0];
            var useCase = new DeleteWordUseCase(words, words, words);

            var result = await useCase.DeleteWordAsync(new DeleteNicknameWordCommand(target.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(target, words.Words);
            Assert.Equal(10, words.Words.Count);
        }

        [Fact]
        public async Task Delete_is_blocked_when_only_10_words_are_left()
        {
            // Spec: the kiosk needs 10 words for suggestions, so the 10th may not be removed.
            AddWords(10);
            var useCase = new DeleteWordUseCase(words, words, words);

            var result = await useCase.DeleteWordAsync(new DeleteNicknameWordCommand(words.Words[0].Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Equal(10, words.Words.Count);
        }

        [Fact]
        public async Task Delete_unknown_word_is_NotFound()
        {
            AddWords(11);
            var useCase = new DeleteWordUseCase(words, words, words);

            var result = await useCase.DeleteWordAsync(new DeleteNicknameWordCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            Assert.Equal(11, words.Words.Count);
        }

        [Fact]
        public async Task Delete_unknown_word_is_NotFound_even_when_few_words_are_left()
        {
            // 404 is checked before the 10-word rule.
            AddWords(3);
            var useCase = new DeleteWordUseCase(words, words, words);

            var result = await useCase.DeleteWordAsync(new DeleteNicknameWordCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Delete_counts_only_active_words()
        {
            // 10 active + 5 inactive: only the active ones count, so the delete is still blocked.
            AddWords(10);
            for (var i = 0; i < 5; i++)
                words.Words.Add(NicknameWord.Create($"Gammel{i}", isActive: false));
            var useCase = new DeleteWordUseCase(words, words, words);

            var result = await useCase.DeleteWordAsync(new DeleteNicknameWordCommand(words.Words[0].Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }
    }
}
