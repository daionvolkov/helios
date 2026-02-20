using Helios.Platform.Common;

namespace Helios.Platform.Agents.Intrefaces;

public interface IAgentEnrollmentManager
{
    Task<DomainResult<(string Token, DateTimeOffset ExpiresAt)>> CreateTokenAsync(
         Guid tenantId,
         Guid serverId,
         Guid? createdByUserId,
         TimeSpan ttl,
         CancellationToken ct);

    Task<DomainResult<(Guid TenantId, Guid ServerId)>> ValidateAndConsumeAsync(
        string plainToken,
        CancellationToken ct);
}
