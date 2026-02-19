namespace Helios.Persistence.Entities;

public sealed class Role
{
    public Guid RoleId { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = new();
}
