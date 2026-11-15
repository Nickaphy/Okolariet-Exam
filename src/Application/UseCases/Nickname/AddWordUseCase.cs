using System;
using System.Collections.Generic;
using System.Text;
using Application.ErrorMessages;
using Application.OutBoundPort.INicknameWordRepository;
using Application.InBoundPort.INicknameUseCases;
using Domain.Entities;
using FluentResults;
using Application.Commands.NicknameCommands;

namespace Application.UseCases.Nickname
{
    public class AddWordUseCase : IAddWordAsync
    {
        private readonly IExistsAsync exists;
        private readonly IAddAsync add;
        public AddWordUseCase(IExistsAsync exists, IAddAsync add)
        {
            this.exists = exists;
            this.add = add;
        }
        public async Task<Result<NicknameWord>> AddWordAsync(AddNicknameWordCommand command, CancellationToken ct)
        {
            if (await exists.ExistsAsync(command.Text, ct))
                return Result.Fail<NicknameWord>(new ConflictError($"The word '{command.Text}' already exists."));

            var word = NicknameWord.Create(command.Text, true);
            await add.AddAsync(word, ct);
            return Result.Ok(word);
        }
    }
}
