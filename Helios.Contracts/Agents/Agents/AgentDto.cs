namespace Helios.Contracts.Agents.Agents;

public sealed class AgentDto
{
    public Guid AgentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ServerId { get; set; }

    public string? Name { get; set; }
    public string Status { get; set; } = "Active";

    public DateTimeOffset? LastSeenAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
