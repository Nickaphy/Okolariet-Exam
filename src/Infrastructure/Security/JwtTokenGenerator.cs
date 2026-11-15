using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Auth;
using Application.OutBoundPort.AuthRepository.Tokens;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security
{
    public sealed class JwtTokenGenerator(IOptions<JwtSettings> options) : IJwtTokenGenerator // This is the actual factory, that creates Jwt tokens. 
    {
        public string Generate(StaffMember staff, Guid loginId, DateTimeOffset expiresAt)
        {
            var s = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Secret));   // same encoding as the Api

            var claims = new[]
            {
            new Claim("sub",   staff.Id.ToString()),
            new Claim("email", staff.Email),
            new Claim("role",  staff.Role.ToString()),   // "Admin"
            new Claim("sid",   loginId.ToString()),
            new Claim("jti",   Guid.NewGuid().ToString()),
        };

            var token = new JwtSecurityToken(
                issuer: s.Issuer,
                audience: s.Audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
