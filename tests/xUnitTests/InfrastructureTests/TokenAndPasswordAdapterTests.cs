using System.IdentityModel.Tokens.Jwt;          // JwtSecurityTokenHandler
using System.Text;
using Application.Auth;                        // JwtSettings
using Domain.Entities;                         // StaffMember
using Domain.Enums;                            // StaffRole
using Infrastructure.Adapters.Auth;            // GenerateRefreshTokenAdapter, HashTokenAdapter
using Infrastructure.Security;                 // JwtTokenGenerator, AspNetPasswordHasher
using Microsoft.Extensions.Options;            // Options.Create
using Microsoft.IdentityModel.Tokens;          // TokenValidationParameters, SymmetricSecurityKey

namespace xUnitTests.InfrastructureTests
{
    public class JwtTokenGeneratorTests
    {
        private const string Secret = "unit-test-secret-that-is-longer-than-32-bytes!";

        private static readonly StaffMember Anna = new()
        {
            Id = Guid.NewGuid(), Email = "anna@test.dk", PasswordHash = "x", Role = StaffRole.Admin
        };

        private static JwtTokenGenerator Generator(string secret = Secret) =>
            new(Options.Create(new JwtSettings { Secret = secret, Issuer = "okolariet-api", Audience = "okolariet-web" }));

        // Same checks the Api does in AddJwtBearer.
        private static TokenValidationParameters ValidationParameters(string secret = Secret) => new()
        {
            ValidIssuer = "okolariet-api",
            ValidAudience = "okolariet-web",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
        };

        [Fact]
        public void Token_contains_the_claims_from_the_spec()
        {
            var loginId = Guid.NewGuid();

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(Generator().Generate(Anna, loginId, DateTimeOffset.UtcNow.AddMinutes(15)));

            Assert.Equal(Anna.Id.ToString(), jwt.Claims.Single(c => c.Type == "sub").Value);
            Assert.Equal("anna@test.dk", jwt.Claims.Single(c => c.Type == "email").Value);
            Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == "role").Value);   // must match [Authorize(Roles = "Admin")]
            Assert.Equal(loginId.ToString(), jwt.Claims.Single(c => c.Type == "sid").Value);
            Assert.Equal("okolariet-api", jwt.Issuer);
            Assert.Equal("okolariet-web", Assert.Single(jwt.Audiences));
        }

        [Fact]
        public void Token_expires_at_the_given_time()
        {
            var expires = DateTimeOffset.UtcNow.AddMinutes(15);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(Generator().Generate(Anna, Guid.NewGuid(), expires));

            // JWT stores whole seconds.
            Assert.Equal(expires.ToUnixTimeSeconds(), new DateTimeOffset(jwt.ValidTo).ToUnixTimeSeconds());
        }

        [Fact]
        public void Token_passes_the_same_validation_the_Api_uses()
        {
            var token = Generator().Generate(Anna, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15));

            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }
                .ValidateToken(token, ValidationParameters(), out _);

            Assert.Equal(Anna.Id.ToString(), principal.FindFirst("sub")?.Value);
        }

        [Fact]
        public void Token_signed_with_another_secret_is_rejected()
        {
            var token = Generator("a-completely-different-secret-also-32-bytes-long").Generate(Anna, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15));

            Assert.ThrowsAny<SecurityTokenException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _));
        }

        [Fact]
        public void Expired_token_is_rejected()
        {
            var token = Generator().Generate(Anna, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-10));

            Assert.Throws<SecurityTokenExpiredException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _));
        }

        [Fact]
        public void Secret_shorter_than_32_bytes_cannot_sign()
        {
            Assert.ThrowsAny<Exception>(() => Generator("too-short").Generate(Anna, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15)));
        }

        [Fact]
        public void Every_token_gets_its_own_jti()
        {
            var handler = new JwtSecurityTokenHandler();
            var expires = DateTimeOffset.UtcNow.AddMinutes(15);

            var first = handler.ReadJwtToken(Generator().Generate(Anna, Guid.NewGuid(), expires)).Id;
            var second = handler.ReadJwtToken(Generator().Generate(Anna, Guid.NewGuid(), expires)).Id;

            Assert.NotEqual(first, second);
        }
    }

    public class AspNetPasswordHasherTests
    {
        private readonly AspNetPasswordHasher hasher = new();

        [Fact]
        public void Hash_is_not_the_password()
        {
            var hash = hasher.Hash("correct horse battery");

            Assert.NotEqual("correct horse battery", hash);
            Assert.DoesNotContain("correct horse battery", hash);
        }

        [Fact]
        public void Same_password_gets_a_different_hash_each_time()
        {
            // A random salt per hash, so two staff with the same password can't be spotted.
            Assert.NotEqual(hasher.Hash("correct horse battery"), hasher.Hash("correct horse battery"));
        }

        [Fact]
        public void Verify_accepts_the_right_password()
        {
            var hash = hasher.Hash("correct horse battery");

            Assert.True(hasher.Verify(hash, "correct horse battery"));
        }

        [Fact]
        public void Verify_rejects_a_wrong_password()
        {
            var hash = hasher.Hash("correct horse battery");

            Assert.False(hasher.Verify(hash, "Correct horse battery"));
        }

        [Fact]
        public void VerifyDummy_always_fails()
        {
            Assert.False(hasher.VerifyDummy("anything"));
            Assert.False(hasher.VerifyDummy("timing-equaliser-not-a-real-password"));
        }
    }

    public class HashTokenAdapterTests
    {
        private readonly HashTokenAdapter adapter = new();

        [Fact]
        public void Hash_is_SHA256_as_uppercase_hex()
        {
            // Known SHA-256 test vector for "abc".
            Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", adapter.HashToken("abc"));
        }

        [Fact]
        public void Same_token_gives_the_same_hash()
        {
            // Needed: the refresh use case looks tokens up by their hash.
            Assert.Equal(adapter.HashToken("some-token"), adapter.HashToken("some-token"));
        }

        [Fact]
        public void Different_tokens_give_different_hashes()
        {
            Assert.NotEqual(adapter.HashToken("token-a"), adapter.HashToken("token-b"));
        }
    }

    public class GenerateRefreshTokenAdapterTests
    {
        private readonly GenerateRefreshTokenAdapter adapter = new();

        [Fact]
        public void Token_is_32_random_bytes_as_url_safe_text()
        {
            var token = adapter.GenerateRefreshToken();

            Assert.Equal(43, token.Length);   // 32 bytes in Base64Url without padding
            Assert.Matches("^[A-Za-z0-9_-]+$", token);
        }

        [Fact]
        public void Tokens_are_unique()
        {
            var tokens = Enumerable.Range(0, 1000).Select(_ => adapter.GenerateRefreshToken()).ToHashSet();

            Assert.Equal(1000, tokens.Count);
        }
    }
}
