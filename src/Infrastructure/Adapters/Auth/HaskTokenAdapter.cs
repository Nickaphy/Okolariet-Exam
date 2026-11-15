using System.Security.Cryptography;
using System.Text;
using Application.OutBoundPort.AuthRepository.Tokens;

namespace Infrastructure.Adapters.Auth
{
    public sealed class HashTokenAdapter : IHashToken
    {
        public string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))); //Hashes the token using SHA256 and returns the hash as a hexadecimal string
    }
}
