namespace Helios.Platform.Servers.Models;

public sealed class UpdateServerModel
{
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Host { get; set; }

    public List<string>? Tags { get; set; }
    public string? Status { get; set; }
}
