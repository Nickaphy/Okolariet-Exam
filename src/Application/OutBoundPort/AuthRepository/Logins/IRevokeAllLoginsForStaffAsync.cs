namespace Application.OutBoundPort.AuthRepository.Logins
{
    public interface IRevokeAllLoginsForStaffAsync
    {
        Task RevokeAllLoginsForStaffAsync(Guid staffId, DateTimeOffset revokedAt, CancellationToken ct = default);
    }
}
