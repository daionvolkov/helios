namespace Helios.Platform.Servers;

public sealed class ServerDto
{
    public Guid ServerId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Hostname { get; set; }

    public List<string> TagsJson { get; set; } = new();

    public ServerStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
