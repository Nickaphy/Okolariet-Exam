using System;
using System.Collections.Generic;
using System.Text;
using Application.Commands.AuthCommands;
using Application.InBoundPort.AuthUseCases;
using Application.OutBoundPort.AuthRepository.Logins;
using FluentResults;

namespace Application.UseCases.Auth
{
    public sealed class LogoutUseCase : ILogoutUseCase // Let´'s the user logout and trashes their token
    {
        private readonly IGetLoginByIdAsync getLoginById;
        private readonly IUpdateLoginAsync updateLogin;
        private readonly TimeProvider time;
        public LogoutUseCase(IGetLoginByIdAsync getLoginById, IUpdateLoginAsync updateLogin, TimeProvider time)
        {
            this.getLoginById = getLoginById;
            this.updateLogin = updateLogin;
            this.time = time;
        }
        public async Task<Result> ExecuteAsync(LogoutCommand command, CancellationToken ct = default)
        {
            var login = await getLoginById.GetLoginByIdAsync(command.LoginId, ct);
            if (login is not null && login.RevokedAt is null)
            {
                login.RevokedAt = time.GetUtcNow();
                await updateLogin.UpdateLoginAsync(login, ct);
            }
            return Result.Ok();   // idempotent: logging out twice is fine
        }
    }
}
