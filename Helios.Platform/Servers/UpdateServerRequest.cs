

namespace Helios.Platform.Servers;

public sealed class UpdateServerRequest
{
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Host { get; set; }

    public List<string>? Tags { get; set; }
    public ServerStatus? Status { get; set; }
}
