namespace Helios.Contracts.Agents.Agents;

public sealed class CreateEnrollmentTokenResponse
{
    public string Token { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}