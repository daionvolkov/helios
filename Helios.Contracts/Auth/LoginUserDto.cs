namespace Helios.Contracts.Auth;

public sealed class LoginUserDto
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
