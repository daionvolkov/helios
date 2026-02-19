namespace Helios.Persistence.Entities;

public sealed class Tenant
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public List<User> Users { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<Server> Servers { get; set; } = new();
}
