using Application.Commands;
using Application.ErrorMessages;
using Application.OutBoundPort.INicknameWordRepository;
using Application.InBoundPort.INicknameUseCases;
using FluentResults;

namespace Application.UseCases.Nickname
{
    public class DeleteWordUseCase : IDeleteWordAsync
    {
        private readonly IGetAsync get;
        private readonly IRemoveAsync remove;
        private readonly ICountActiveAsync countActive;
        public DeleteWordUseCase(IGetAsync get, IRemoveAsync remove, ICountActiveAsync countActive)
        {
            this.get = get;
            this.remove = remove;
            this.countActive = countActive;
        }
        public async Task<Result> DeleteWordAsync(DeleteNicknameWordCommand command, CancellationToken ct)
        {
            var word = await get.GetAsync(command.Id, ct);
            if (word is null)
                return Result.Fail(new NotFoundError("Nickname word not found."));

            if (await countActive.CountActiveAsync(ct) <= 10)   // spec: the kiosk needs 10 words
                return Result.Fail(new BusinessRuleError("At least 10 words must remain."));

            await remove.RemoveAsync(word, ct);
            return Result.Ok();
        }
    }
}
