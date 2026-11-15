namespace Application.OutBoundPort.IStaffRepository
{
    public interface ICountAdminsAsync
    {
        Task<int> CountAdminsAsync(CancellationToken ct);
    }
}
