namespace Helios.Persistence.Entities;

public sealed class UserRole
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
