namespace Helios.Contracts.Agents.Enrollment;

public sealed class AgentEnrollRequest
{
    public string Token { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string AgentVersion { get; set; } = null!;
    public string Os { get; set; } = null!;
    public string Arch { get; set; } = null!;

    public string? CapabilitiesJson { get; set; } // jsonb
}
