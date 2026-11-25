using Domain.ValueObjects;

namespace xUnitTests.DomainTests
{
    public class ScoreTests
    {
        [Theory]
        [InlineData(0, 4, 0)]
        [InlineData(1, 4, 25)]
        [InlineData(4, 4, 100)]
        public void Percentage_matches_correct_answers(int correct, int total, double percentage)
        {
            var score = new Score(correct, total);

            Assert.Equal(percentage, score.Percentage);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, 4)]
        [InlineData(5, 4)]
        public void Invalid_score_throws(int correct, int total)
        {
            Assert.Throws<ArgumentException>(() => new Score(correct, total));
        }
    }
}
