namespace Application.OutBoundPort.INicknameWordRepository
{
    public interface IExistsAsync
    {
        Task<bool> ExistsAsync(string text, CancellationToken ct);
    }
}
