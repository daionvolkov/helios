using Helios.Contracts.Auth;
using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Helios.Identity.Services;

public sealed class LoginService : ILoginService
{
    private readonly HeliosDbContext _db;
    private readonly PasswordHasher<User> _hasher;
    private readonly IJwtTokenService _tokens;

    public LoginService(HeliosDbContext db, PasswordHasher<User> hasher, IJwtTokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }


    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);

        if (user == null || !user.IsActive)
            return null;

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return null;

        var roles = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.RoleId
            where ur.UserId == user.UserId && ur.TenantId == user.TenantId
            select r.Code
        ).ToListAsync(ct);

        var token = _tokens.IssueToken(user, user.TenantId, roles);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = new LoginUserDto
            {
                UserId = user.UserId,
                TenantId = user.TenantId,
                Email = user.Email,
                Roles = roles
            }
        };
    }
}
