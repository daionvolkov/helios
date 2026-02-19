using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Helios.Identity.Options;
using Helios.Persistence.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace Helios.Identity.Services
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opt;
        public JwtTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

        public TokenIssueResult IssueToken(User user, Guid tenantId, IReadOnlyCollection<string> roles)
        {
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(_opt.AccessTokenMinutes);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new("tid", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            foreach (var r in roles.Distinct(StringComparer.OrdinalIgnoreCase))
                claims.Add(new Claim(ClaimTypes.Role, r));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: creds);

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenIssueResult(tokenStr, expires);
        }
    }
}
