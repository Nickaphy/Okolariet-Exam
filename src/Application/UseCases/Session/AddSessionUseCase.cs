using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.InBoundPort; // IPostSessionAsync
using Application.OutBoundPort.ISessionRepository; // IProjectExists
using Application.Commands.Sessions; // CreateSessionCommand
using FluentResults; 
using Domain.Entities; 
using Application.ErrorMessages; // NotFoundError

namespace Application.UseCases.Sessions
{
    public class AddSessionUseCase : IPostSessionAsync
    {
        // Outbound port responsible for getting the existence of a session from the infrastructure back to AddSessionUsecase.
        private readonly IProjectExists projectExists;
        private readonly ICreateSessionAsync createSessionAsync;
        
        // Contructor injection of the outbound port
        public AddSessionUseCase(IProjectExists projectExists, ICreateSessionAsync createSessionAsync)
        {
            this.projectExists = projectExists;
            this.createSessionAsync = createSessionAsync;
        }
        
        public async Task<Result<Session>> PostSessionAsync(CreateSessionCommand command, CancellationToken ct)
        {
            if (!await projectExists.ProjectExistsAsync(command.ProjectId, ct)) // does this project already exist in the database?
                return Result.Fail<Session>(new NotFoundError($"The project for projectId '{command.ProjectId}' does not exist."));

            var session = Session.Create(command.ProjectId); // Create a new session entity with the provided projectId from the command (record)
            await createSessionAsync.CreateSessionAsync(session, ct); // Persist the new session entity to the database using the outbound port
            return Result.Ok(session); // Return the newly created session entity wrapped in a Result object
        }
    }
}