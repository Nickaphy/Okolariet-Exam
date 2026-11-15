using System.Buffers.Text;
using System.Security.Cryptography;
using Application.OutBoundPort.AuthRepository.Tokens;

namespace Infrastructure.Adapters.Auth
{
    public sealed class GenerateRefreshTokenAdapter : IGenerateRefreshToken
    {
        public string GenerateRefreshToken() =>
            Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32)); // Generates a random 32-byte array and encodes it to a Base64 URL-safe string. A JWT token needs to be atleast 32 characters long.
    }
}
