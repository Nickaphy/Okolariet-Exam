using Domain.ValueObjects;   // Appearance

namespace xUnitTests.DomainTests
{
    public class AppearanceTests
    {
        [Fact]
        public void Valid_colours_and_font_are_kept()
        {
            var appearance = new Appearance("#0A6E8A", "#ffffff", "#000000", "Nunito");

            Assert.Equal("#0A6E8A", appearance.BackgroundColor);
            Assert.Equal("#ffffff", appearance.ButtonColor);
            Assert.Equal("#000000", appearance.TextColor);
            Assert.Equal("Nunito", appearance.FontFamily);
        }

        [Fact]
        public void All_null_is_allowed_and_equals_Empty()
        {
            var appearance = new Appearance(null, null, null, null);

            Assert.Equal(Appearance.Empty, appearance);   // records compare by value
        }

        [Theory]
        [InlineData("blue")]
        [InlineData("#12345")]      // too short
        [InlineData("#1234567")]    // too long
        [InlineData("0A6E8A")]      // missing #
        [InlineData("#GGGGGG")]     // not hex
        [InlineData("red; display:none")]
        public void Invalid_colour_throws(string colour)
        {
            Assert.Throws<ArgumentException>(() => new Appearance(colour, null, null, null));
            Assert.Throws<ArgumentException>(() => new Appearance(null, colour, null, null));
            Assert.Throws<ArgumentException>(() => new Appearance(null, null, colour, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Blank_font_throws(string font)
        {
            Assert.Throws<ArgumentException>(() => new Appearance(null, null, null, font));
        }
    }
}
