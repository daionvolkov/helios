using Helios.Core.Auth;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Helios.App.Auth;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;
    public HttpTenantContext(IHttpContextAccessor http) => _http = http;

    public Guid TenantId
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Unauthenticated request.");

            var tid = user.FindFirstValue(TenantClaimNames.TenantId);
            if (string.IsNullOrWhiteSpace(tid) || !Guid.TryParse(tid, out var tenantId))
                throw new UnauthorizedAccessException("Missing tenantId claim (tid).");

            return tenantId;
        }
    }

    public Guid? UserId
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
    }


    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return Array.Empty<string>();

            return user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
        }
    }
}
