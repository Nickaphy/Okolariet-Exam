namespace Application.OutBoundPort.IStaffRepository
{
    public interface IEmailExistsAsync
    {
        Task<bool> EmailExistsAsync(string normalizedEmail, Guid? exceptId, CancellationToken ct);
    }
}
