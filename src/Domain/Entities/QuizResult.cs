using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.ValueObjects;


namespace Domain.Entities
{
    public class QuizResult : Entity
    {
        public Guid SessionId {get;}
        public Guid QuestionId{get;}
        public Guid AnswerId{get;}
        public bool IsCorrect{get;}


        // Constructor
        private QuizResult(
            Guid sessionId,
            Guid questionId,
            Guid answerId,
            bool isCorrect)
        {
            SessionId = sessionId;
            QuestionId = questionId;
            AnswerId = answerId;
            IsCorrect = isCorrect;
        }

        // Factory method
        public static QuizResult Create(
            Guid sessionId,
            Question question,
            Answer answer
        )
        {
            // Rejects ampty sessionId
            if (sessionId == Guid.Empty)
                throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
            // Question cannot be null
            ArgumentNullException.ThrowIfNull(question);
            // Answer cannot be null
            ArgumentNullException.ThrowIfNull(answer);

            // The Answer must belong to the question
            if (answer.QuestionId != question.Id)
                throw new ArgumentException("The answer must belong to the question", nameof(answer));

            // Return the QuizResult entity
            return new QuizResult(
                sessionId,
                question.Id,
                answer.Id,
                answer.IsCorrect);

        }
    }
}