namespace Helios.Contracts.Agents.Enrollment;

public sealed class AgentEnrollResponse
{
    public Guid AgentId { get; set; }
    public string AccessKeyId { get; set; } = null!;
    public string Secret { get; set; } = null!;

    public DateTimeOffset IssuedAt { get; set; }
}
