using System;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Session : Aggregateroot
    {
        public Guid ProjectId {get; private set;} // Unique identifier for the project associated with the session
        public DateTime StartedAt {get;} // Immutable timestamp indicating when the session started


        // Private constructor to enforce the use of the factory method for creating Session instances
        private Session(Guid projectId, DateTime startedAt)
        {
            ProjectId = projectId;
            StartedAt = startedAt;
        }

        // Factory method to create a new Session instance with validation
        public static Session Create(Guid projectId)
        {
            // Validate that the projectId is not empty
            if (projectId == Guid.Empty)
                throw new ArgumentException("ProjectId cannot be an empty GUID.", nameof(projectId));

            // Create a new Session object with the provided projectId and the current time
            return new Session(projectId, DateTime.UtcNow);
        }
    }
}