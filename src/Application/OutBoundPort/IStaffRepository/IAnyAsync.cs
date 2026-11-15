namespace Application.OutBoundPort.IStaffRepository
{
    // Used for seeding the first admin
    public interface IAnyAsync
    {
        Task<bool> AnyAsync(CancellationToken ct);
    }
}
