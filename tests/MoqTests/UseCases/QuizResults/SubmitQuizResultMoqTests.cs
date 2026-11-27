using Application;                                          // SubmittedQuizAnswer
using Application.Commands.QuizResultCommands;              // SubmitQuizResultCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError, ConflictError
using Application.OutBoundPort.IModdedTemplateRepository;   // ITemplateBelongsToProjectAsync
using Application.OutBoundPort.IQuestionRepository;         // IGetQuestionByIdAsync
using Application.OutBoundPort.IQuizResultRepository;       // ITryAddQuizResultAsync
using Application.OutBoundPort.ISessionRepository;          // IGetSessionByIdAsync
using Application.UseCases.QuizResults;                     // SubmitQuizResultUseCase
using Domain.Entities;                                      // Session, Question, Answer, QuizResult
using FluentResults;                                        // Result
using Moq;

namespace MoqTests.UseCases.QuizResults
{
    // Spec (contracts/openapi.yaml, POST /results): the server checks correctness.
    // The question must belong to the session's project and be active, and the answer must belong
    // to that question (otherwise 422). A question can be answered once per session (otherwise 409).
    public class SubmitQuizResultMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetSessionByIdAsync> getSession = new();
        private readonly Mock<IGetQuestionByIdAsync> getQuestion = new();
        private readonly Mock<ITemplateBelongsToProjectAsync> belongs = new();
        private readonly Mock<ITryAddQuizResultAsync> addResult = new();

        private readonly Guid projectId = Guid.NewGuid();
        private readonly Session session;
        private readonly Question question;
        private readonly Answer correct;
        private readonly Answer wrong;

        public SubmitQuizResultMoqTests()
        {
            session = Session.Create(projectId);
            (question, correct, wrong) = ExistingQuestion();
            getSession.Setup(g => g.GetSessionByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
            addResult.Setup(a => a.TryAddAsync(It.IsAny<QuizResult>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private SubmitQuizResultUseCase CreateUseCase() => new(getSession.Object, getQuestion.Object, belongs.Object, addResult.Object);

        private Task<Result<SubmittedQuizAnswer>> Submit(Guid answerId, Guid? questionId = null)
            => CreateUseCase().SubmitAsync(new SubmitQuizResultCommand(session.Id, questionId ?? question.Id, answerId), Ct);

        // A question in this session's project with one correct and one wrong answer, "in the database".
        private (Question, Answer Correct, Answer Wrong) ExistingQuestion(bool inSessionsProject = true)
        {
            var q = Question.Create(Guid.NewGuid(), "Hvad spiser en sæl?", null, 1);
            var c = q.AddAnswer("Fisk", true, null, 1);
            var w = q.AddAnswer("Græs", false, null, 2);
            getQuestion.Setup(g => g.GetQuestionByIdAsync(q.Id, It.IsAny<CancellationToken>())).ReturnsAsync(q);
            belongs.Setup(b => b.BelongsToProjectAsync(q.ModdedTemplateId, projectId, It.IsAny<CancellationToken>())).ReturnsAsync(inSessionsProject);
            return (q, c, w);
        }

        private void AssertNothingSaved()
            => addResult.Verify(a => a.TryAddAsync(It.IsAny<QuizResult>(), It.IsAny<CancellationToken>()), Times.Never);

        // ---------- Success ----------

        [Fact]
        public async Task Correct_answer_is_saved_as_correct()
        {
            var result = await Submit(correct.Id);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.RecordResult.IsCorrect);
            Assert.Equal(correct.Id, result.Value.CorrectAnswerId);
            addResult.Verify(a => a.TryAddAsync(
                It.Is<QuizResult>(r => r.SessionId == session.Id && r.QuestionId == question.Id && r.AnswerId == correct.Id),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Wrong_answer_is_saved_as_wrong_and_the_correct_answer_is_returned()
        {
            var result = await Submit(wrong.Id);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.RecordResult.IsCorrect);
            Assert.Equal(correct.Id, result.Value.CorrectAnswerId);
        }

        [Fact]
        public async Task Answering_the_same_question_twice_is_Conflict()
        {
            addResult.Setup(a => a.TryAddAsync(It.IsAny<QuizResult>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await Submit(correct.Id);

            Assert.True(result.HasError<ConflictError>());
        }

        // ---------- 404 ----------

        [Fact]
        public async Task Unknown_session_is_NotFound()
        {
            var result = await CreateUseCase().SubmitAsync(new SubmitQuizResultCommand(Guid.NewGuid(), question.Id, correct.Id), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unknown_question_is_NotFound()
        {
            var result = await Submit(correct.Id, questionId: Guid.NewGuid());

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        // ---------- 422 ----------

        [Fact]
        public async Task Deleted_question_is_BusinessRuleError()
        {
            question.Deactivate();

            var result = await Submit(correct.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Question_from_another_project_is_BusinessRuleError()
        {
            var (otherQuestion, otherCorrect, _) = ExistingQuestion(inSessionsProject: false);

            var result = await Submit(otherCorrect.Id, questionId: otherQuestion.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Answer_from_another_question_is_BusinessRuleError()
        {
            var (_, answerToOtherQuestion, _) = ExistingQuestion();

            var result = await Submit(answerToOtherQuestion.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Deleted_answer_is_BusinessRuleError()
        {
            wrong.Deactivate();

            var result = await Submit(wrong.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Question_without_a_correct_answer_is_BusinessRuleError()
        {
            correct.Deactivate();   // only the wrong answer is left

            var result = await Submit(wrong.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Question_with_two_correct_answers_is_BusinessRuleError()
        {
            var (twoCorrect, first, _) = ExistingQuestion();
            twoCorrect.AddAnswer("Blæksprutte", true, null, 3);

            var result = await Submit(first.Id, questionId: twoCorrect.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }
    }
}
