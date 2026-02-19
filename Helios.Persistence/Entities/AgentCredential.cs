namespace Helios.Persistence.Entities;

public sealed class AgentCredential
{
    public Guid TenantId { get; set; }
    public Guid AgentId { get; set; }

    public string AccessKeyId { get; set; } = null!;
    public byte[] AccessKeyHash { get; set; } = null!;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public Agent Agent { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
