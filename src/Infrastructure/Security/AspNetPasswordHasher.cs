using Application.OutBoundPort.IPasswordHash;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security
{
    public class AspNetPasswordHasher : IHash, IVerify, IVerifyDummy
    {
        private static readonly PasswordHasher<object> Inner = new();
        private static readonly object Owner = new();   // the built-in hasher ignores this argument
        private static readonly string DummyHash = Inner.HashPassword(Owner, "timing-equaliser-not-a-real-password");

        public string Hash(string password) => Inner.HashPassword(Owner, password);

        public bool Verify(string passwordHash, string password) =>
            Inner.VerifyHashedPassword(Owner, passwordHash, password) != PasswordVerificationResult.Failed;

        public bool VerifyDummy(string password)
        {
            Inner.VerifyHashedPassword(Owner, DummyHash, password);
            return false;
        }
    }
}
