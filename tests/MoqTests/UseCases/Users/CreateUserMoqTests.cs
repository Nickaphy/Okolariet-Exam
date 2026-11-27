using Application.Commands.UserCommands;                    // CreateUserCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError, ConflictError
using Application.OutBoundPort.IModdedTemplateRepository;   // ITemplateBelongsToProjectAsync
using Application.OutBoundPort.INicknameWordRepository;     // IGetAsync
using Application.OutBoundPort.IQuestionRepository;         // IListQuestionsByTemplateAsync
using Application.OutBoundPort.IQuizResultRepository;       // IListQuizResultsBySessionAsync
using Application.OutBoundPort.ISessionRepository;          // IGetSessionByIdAsync
using Application.OutBoundPort.IUserRepository;             // ICreateUserAsync
using Application.UseCases.Users;                           // CreateUserUseCase
using Domain.Entities;                                      // User, Session, NicknameWord, Question, Answer, QuizResult
using FluentResults;                                        // Result
using Moq;

namespace MoqTests.UseCases.Users
{
    // Spec (contracts/openapi.yaml, POST /users): the server builds the nickname from the two words
    // (capitalised, in the order given) followed by the number, e.g. "ModigRæv42", and calculates the score itself.
    // Rules (otherwise 422): both words active and different, number 0-99, quiz in the session's project,
    // every question answered. One result per quiz per session (otherwise 409).
    public class CreateUserMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<ICreateUserAsync> createUser = new();
        private readonly Mock<IGetAsync> getWord = new();
        private readonly Mock<IGetSessionByIdAsync> getSession = new();
        private readonly Mock<ITemplateBelongsToProjectAsync> belongs = new();
        private readonly Mock<IListQuestionsByTemplateAsync> listQuestions = new();
        private readonly Mock<IListQuizResultsBySessionAsync> listResults = new();

        private readonly Guid projectId = Guid.NewGuid();
        private readonly Guid quizId = Guid.NewGuid();
        private readonly Session session;
        private readonly NicknameWord modig = NicknameWord.Create("modig");
        private readonly NicknameWord raev = NicknameWord.Create("ræv");

        // The quiz has two questions. Each test answers them with Answer(...).
        private readonly List<Question> questions = new();
        private readonly List<QuizResult> results = new();
        private readonly Question q1;
        private readonly Question q2;

        public CreateUserMoqTests()
        {
            session = Session.Create(projectId);
            q1 = AddQuestion(quizId);
            q2 = AddQuestion(quizId);

            getWord.Setup(g => g.GetAsync(modig.Id, It.IsAny<CancellationToken>())).ReturnsAsync(modig);
            getWord.Setup(g => g.GetAsync(raev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(raev);
            getSession.Setup(g => g.GetSessionByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
            belongs.Setup(b => b.BelongsToProjectAsync(quizId, projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            listQuestions.Setup(l => l.ListQuestionsByTemplateAsync(quizId, It.IsAny<CancellationToken>())).ReturnsAsync(questions);
            listResults.Setup(l => l.ListBySessionAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(results);
            createUser.Setup(c => c.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private CreateUserUseCase CreateUseCase() => new(
            createUser.Object, getWord.Object, getSession.Object, belongs.Object, listQuestions.Object, listResults.Object);

        private Task<Result<User>> Save(Guid? firstWord = null, Guid? secondWord = null, int number = 42, Guid? sessionId = null)
            => CreateUseCase().PostUserAsync(
                new CreateUserCommand(sessionId ?? session.Id, quizId, firstWord ?? modig.Id, secondWord ?? raev.Id, number), Ct);

        // Adds a question with one correct and one wrong answer to the given quiz.
        private Question AddQuestion(Guid toQuiz)
        {
            var question = Question.Create(toQuiz, $"Spørgsmål {questions.Count + 1}", null, questions.Count + 1);
            question.AddAnswer("Rigtigt", true, null, 1);
            question.AddAnswer("Forkert", false, null, 2);
            if (toQuiz == quizId)
                questions.Add(question);
            return question;
        }

        // Records that this session answered the question, correctly or not.
        private void Answer(Question question, bool correctly)
            => results.Add(QuizResult.Create(session.Id, question, question.Answers.Single(a => a.IsCorrect == correctly)));

        private void AnswerAll()
        {
            Answer(q1, correctly: true);
            Answer(q2, correctly: false);
        }

        private void AssertNothingSaved()
            => createUser.Verify(c => c.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);

        // ---------- Success ----------

        [Fact]
        public async Task Finished_quiz_is_saved_with_nickname_and_server_calculated_score()
        {
            AnswerAll();

            var result = await Save();

            Assert.True(result.IsSuccess);
            Assert.Equal("ModigRæv42", result.Value.NickName);
            Assert.Equal(1, result.Value.Score.Correct);
            Assert.Equal(2, result.Value.Score.TotalQuestions);
            Assert.Equal(quizId, result.Value.ModdedTemplateId);
            Assert.Equal(session.Id, result.Value.SessionId);
            createUser.Verify(c => c.CreateUserAsync(result.Value, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Words_are_used_in_the_order_given()
        {
            AnswerAll();

            var result = await Save(firstWord: raev.Id, secondWord: modig.Id);

            Assert.Equal("RævModig42", result.Value.NickName);
        }

        [Theory]
        [InlineData(0, "ModigRæv0")]
        [InlineData(99, "ModigRæv99")]
        public async Task Numbers_0_and_99_are_allowed(int number, string expectedNickname)
        {
            AnswerAll();

            var result = await Save(number: number);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedNickname, result.Value.NickName);
        }

        [Fact]
        public async Task Deleted_questions_do_not_count()
        {
            var deleted = AddQuestion(quizId);
            deleted.Deactivate();
            AnswerAll();   // the deleted question is not answered

            var result = await Save();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Score.TotalQuestions);
        }

        [Fact]
        public async Task Answers_to_other_quizzes_do_not_count()
        {
            var otherQuizQuestion = AddQuestion(Guid.NewGuid());
            Answer(otherQuizQuestion, correctly: true);
            Answer(q1, correctly: false);
            Answer(q2, correctly: false);

            var result = await Save();

            Assert.Equal(0, result.Value.Score.Correct);
            Assert.Equal(2, result.Value.Score.TotalQuestions);
        }

        // ---------- 422 ----------

        [Fact]
        public async Task Same_word_twice_is_BusinessRuleError()
        {
            AnswerAll();

            var result = await Save(firstWord: modig.Id, secondWord: modig.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            getWord.Verify(g => g.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            AssertNothingSaved();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public async Task Number_outside_0_to_99_is_BusinessRuleError(int number)
        {
            AnswerAll();

            var result = await Save(number: number);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Inactive_word_is_BusinessRuleError()
        {
            var inactive = NicknameWord.Create("dum", isActive: false);
            getWord.Setup(g => g.GetAsync(inactive.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inactive);
            AnswerAll();

            var result = await Save(secondWord: inactive.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unknown_word_is_BusinessRuleError()
        {
            AnswerAll();

            var result = await Save(secondWord: Guid.NewGuid());

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Quiz_from_another_project_is_BusinessRuleError()
        {
            belongs.Setup(b => b.BelongsToProjectAsync(quizId, projectId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            AnswerAll();

            var result = await Save();

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unanswered_question_is_BusinessRuleError()
        {
            Answer(q1, correctly: true);   // q2 is not answered

            var result = await Save();

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Quiz_without_active_questions_is_BusinessRuleError()
        {
            q1.Deactivate();
            q2.Deactivate();

            var result = await Save();

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        // ---------- 404 / 409 ----------

        [Fact]
        public async Task Unknown_session_is_NotFound()
        {
            var result = await Save(sessionId: Guid.NewGuid());

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Second_save_for_the_same_quiz_and_session_is_Conflict()
        {
            createUser.Setup(c => c.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            AnswerAll();

            var result = await Save();

            Assert.True(result.HasError<ConflictError>());
        }
    }
}
