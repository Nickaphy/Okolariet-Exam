using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Domain.Entities;
using Application.Commands.UserCommands; // CreateUserCommand
using Application.InBoundPort.IUserUsecases; // Implementation of the inbound port for creating a user
using Application.OutBoundPort.IUserRepository; //Outbound persistence
using Application.OutBoundPort.INicknameWordRepository; //Outbound persistence for nicknames

namespace Application.UseCases.Users
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly ICreateUserAsync createUserAsync;
        private readonly IGetAsync getWord;

        public CreateUserUseCase(ICreateUserAsync createUserAsync, IGetAsync getWord)
        {
            this.createUserAsync = createUserAsync;
            this.getWord = getWord;
        }

        // Implementation of the PostUserAsync method from the ICreateUserUseCase interface
        public async Task<Result<User>> PostUserAsync(CreateUserCommand command, CancellationToken ct)
        {
            //Assembling the Nickname from the ID's
            var firstWord = await getWord.GetAsync(command.FirstWordId, ct); // Retrieve the first word entity from the database using the provided ID
            var secondWord = await getWord.GetAsync(command.SecondWordId, ct); // Retrieve the second word entity from the database using the provided ID
            var nickname = $"{firstWord}{secondWord}{command.Number}"; // Concatenate the first and second words to form the nickname                                 

            // Quiz result implementation needs to be finished before User can be finished.
    
            var user = User.Create(nickname, command.ModdedTemplateId, command.SessionId, score); // Create a new user entity with the provided name and email from the command (record)
            await createUserAsync.CreateUserAsync(user, ct); // Persist the new user entity to the database using the outbound port
            return Result.Ok(user); // Return the newly created user entity wrapped in a Result object
            
        }
    }
}