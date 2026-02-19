namespace Helios.Persistence.Entities;

public sealed class ProjectEnvironment
{
    public Guid EnvironmentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public List<Server> Servers { get; set; } = new();
}
