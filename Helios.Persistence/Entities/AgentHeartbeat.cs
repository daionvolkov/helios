namespace Helios.Persistence.Entities;

public sealed class AgentHeartbeat
{
    public Guid HeartbeatId { get; set; }
    public Guid TenantId { get; set; }
    public Guid AgentId { get; set; }
    public Guid ServerId { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
    public string? PayloadJson { get; set; } // jsonb

    public Agent Agent { get; set; } = null!;
    public Server Server { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
