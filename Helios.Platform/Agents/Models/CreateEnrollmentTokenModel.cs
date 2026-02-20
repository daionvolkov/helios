namespace Helios.Platform.Agents.Models;

public sealed class CreateEnrollmentTokenModel
{
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(10);
}
