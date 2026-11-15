namespace Application.OutBoundPort.AuthRepository.Tokens
{
    public interface IHashToken
    {
        string HashToken(string token);
    }
}
