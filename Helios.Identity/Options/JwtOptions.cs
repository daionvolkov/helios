namespace Helios.Identity.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "helios";
    public string Audience { get; set; } = "helios";
    public string SigningKey { get; set; } = null!;
    public int AccessTokenMinutes { get; set; } = 60;
}
