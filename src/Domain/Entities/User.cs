using System;
using Domain.ValueObjects;

namespace Domain.Entities
{
    /* 
        This is an aggregate entity which is why you dont see the userId here,
        But the userId is with us here because its already declared in the Aggregateroot
    */ 

    public class User : Aggregateroot
    {
        public string NickName {get; private set;}
        public Guid ModdedTemplateId {get; private set;} // Unique identifier for the modded template associated with the user
        public Guid SessionId {get; private set;} // Unique identifier for the user's session
        public Score Score {get; private set;} // Value object representing the user's score
        public DateTimeOffset CreatedAt { get; private set; } // Timestamp for sorting


        // Private constructor to enforce the use of the factory method for creating User instances
        private User(string nickname, Guid moddedTemplateId, Guid sessionId, Score score)
        {
            NickName = nickname;
            ModdedTemplateId = moddedTemplateId;
            SessionId = sessionId;
            Score = score;
            CreatedAt = DateTimeOffset.UtcNow;

        }

        // Factory method to create a new User instance with validation
        public static User Create(string nickname, Guid moddedTemplateId, Guid sessionId, Score score)
        {
            // Nickname cannot legally be blank
            if(string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("NickName cannot be blank.", nameof(nickname));
            
            // There has to be a score
            ArgumentNullException.ThrowIfNull(score);
            
            // Return a new User object with the provided values after validation
            return new User(nickname, moddedTemplateId, sessionId, score);
        }
        
    }
}