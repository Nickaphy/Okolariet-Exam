using Domain.Entities;

namespace xUnitTests.DomainTests
{
    public class NicknameWordTests
    {
        [Fact]
        public void Create_sets_text_and_is_active_by_default()
        {
            var word = NicknameWord.Create("Modig");

            Assert.Equal("Modig", word.Text);
            Assert.True(word.IsActive);
            Assert.NotEqual(Guid.Empty, word.Id);
        }

        [Fact]
        public void Create_can_make_an_inactive_word()
        {
            var word = NicknameWord.Create("Modig", isActive: false);

            Assert.False(word.IsActive);
        }

        [Fact]
        public void Two_words_get_different_ids()
        {
            var first = NicknameWord.Create("Modig");
            var second = NicknameWord.Create("Modig");

            Assert.NotEqual(first.Id, second.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_without_text_throws(string? text)
        {
            Assert.Throws<ArgumentException>(() => NicknameWord.Create(text!));
        }
    }
}
