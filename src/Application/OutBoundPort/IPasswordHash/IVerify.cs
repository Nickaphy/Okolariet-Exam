namespace Application.OutBoundPort.IPasswordHash
{
    public interface IVerify
    {
        bool Verify(string passwordHash, string password);
    }
}
