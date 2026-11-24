using Domain.Entities;
using Domain.ValueObjects;

namespace xUnitTests.DomainTests
{
    public class UserTests
    {
        [Fact]
        public void Create_sets_nickname_quiz_session_and_score()
        {
            var quizId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var score = new Score(1, 2);

            var user = User.Create("BlåKat7", quizId, sessionId, score);

            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal("BlåKat7", user.NickName);
            Assert.Equal(quizId, user.ModdedTemplateId);
            Assert.Equal(sessionId, user.SessionId);
            Assert.Same(score, user.Score);
        }

        [Fact]
        public void Create_with_blank_nickname_throws()
        {
            Assert.Throws<ArgumentException>(() =>
                User.Create(" ", Guid.NewGuid(), Guid.NewGuid(), new Score(1, 2)));
        }
    }
}
