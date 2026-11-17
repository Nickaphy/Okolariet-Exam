using System;

namespace Domain.ValueObjects
{
    public class Score
    {
        // We use only {get} here because it ensures that the values are IMMUTABLE (unchangable) after the object is created,
        // {private set} would have allowed the User valueobject to alter its values after creation.
        public int Correct {get;} 
        public int TotalQuestions {get;}
        public double Percentage => 100.0 * Correct / TotalQuestions; // Deriving the percentage from the correct answers and total questions.


        // Constructor
        public Score(int correct, int totalQuestions)
        {
            // Total questions must be greater than zero to avoid division by zero and to ensure a valid score.
            if (totalQuestions <= 0)
                throw new ArgumentException("Total questions must be greater than zero.", nameof(totalQuestions));

            // Correct answers must be more than 0, and correct answers must be less than or equal to total questions
            if (correct < 0 || correct > totalQuestions)
                throw new ArgumentException("Correct answers must be between 0 and total questions.", nameof(correct));

            // Assign the values to the properties after validation
            Correct = correct;
            TotalQuestions = totalQuestions;
        }
    }
}
