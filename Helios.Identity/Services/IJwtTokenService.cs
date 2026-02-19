using Helios.Persistence.Entities;

namespace Helios.Identity.Services;

public interface IJwtTokenService
{
    TokenIssueResult IssueToken(User user, Guid tenantId, IReadOnlyCollection<string> roles);
}

public sealed record TokenIssueResult(string AccessToken, DateTimeOffset ExpiresAt);
