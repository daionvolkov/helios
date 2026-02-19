namespace Helios.Contracts.Tenants;

public sealed class TenantMeResponse
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
