namespace Helios.Persistence.Entities;

public sealed class Project
{
    public Guid ProjectId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<ProjectEnvironment> Environments { get; set; } = new();
    public List<Server> Servers { get; set; } = new();
}
