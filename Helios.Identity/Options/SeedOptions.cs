namespace Helios.Identity.Options;

public sealed class SeedOptions
{
    public string TenantName { get; set; } = "Default";
    public string AdminEmail { get; set; } = "admin@helios.local";
    public string AdminPassword { get; set; } = "ChangeMe_123!";
}
