using Helios.Persistence.Enums;

namespace Helios.Persistence.Entities;

public sealed class Command
{
    public Guid CommandId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ServerId { get; set; }
    public Guid? AgentId { get; set; }

    public string Type { get; set; } = null!;
    public string? PayloadJson { get; set; } // jsonb
    public CommandStatus Status { get; set; }
    public Guid CorrelationId { get; set; }
    public string? IdempotencyKey { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Server Server { get; set; } = null!;
    public Agent? Agent { get; set; }
    public User? CreatedByUser { get; set; }
    public CommandResult? Result { get; set; }
}
