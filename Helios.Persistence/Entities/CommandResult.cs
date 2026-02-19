namespace Helios.Persistence.Entities;

public sealed class CommandResult
{
    public Guid CommandId { get; set; }
    public Guid TenantId { get; set; }

    public string Status { get; set; } = null!; // "Succeeded" | "Failed"
    public int? ExitCode { get; set; }
    public string? Stdout { get; set; }
    public string? Stderr { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public Command Command { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
