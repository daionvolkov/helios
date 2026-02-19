namespace Helios.Persistence.Entities;

public sealed class Server
{
    public Guid ServerId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string Name { get; set; } = null!;
    public string? Hostname { get; set; }
    public string? TagsJson { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Project? Project { get; set; }
    public ProjectEnvironment? Environment { get; set; }
    public List<Agent> Agents { get; set; } = new();
}
