namespace Helios.Persistence.Entities;

public sealed class AgentEnrollmentToken
{
    public Guid TokenId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ServerId { get; set; }

    public byte[] TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Server Server { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
