namespace Helios.Contracts.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public LoginUserDto User { get; set; } = null!;
}
