using Helios.Persistence.Enums;

namespace Helios.Persistence.Entities;

public sealed class Agent
{
    public Guid AgentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ServerId { get; set; }

    public string DisplayName { get; set; } = null!;
    public string AgentVersion { get; set; } = null!;
    public string Os { get; set; } = null!;
    public string Arch { get; set; } = null!;
    public string? CapabilitiesJson { get; set; } // jsonb
    public AgentStatus Status { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Server Server { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public AgentCredential? Credential { get; set; }
    public List<AgentHeartbeat> Heartbeats { get; set; } = new();
}
