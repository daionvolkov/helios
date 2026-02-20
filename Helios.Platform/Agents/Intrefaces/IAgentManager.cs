using Helios.Persistence.Entities;
using Helios.Platform.Common;

namespace Helios.Platform.Agents.Intrefaces;

public interface IAgentManager
{
    Task<DomainResult<(Guid AgentId, string AccessKeyId, string SecretPlain)>> CreateAgentForServerAsync(
         Guid tenantId,
         Guid serverId,
         string displayName,
         string agentVersion,
         string os,
         string arch,
         string? capabilitiesJson,
         CancellationToken ct);

    Task<DomainResult<IReadOnlyList<Agent>>> GetAgentsByServerAsync(
        Guid tenantId,
        Guid serverId,
        CancellationToken ct);

    Task<DomainResult<Agent>> GetAgentByIdAsync(
        Guid tenantId,
        Guid agentId,
        CancellationToken ct);
}
