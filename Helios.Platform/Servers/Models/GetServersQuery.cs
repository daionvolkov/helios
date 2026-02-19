namespace Helios.Platform.Servers.Models;

public sealed class GetServersQuery
{
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string? Search { get; set; }

    public List<string>? Tags { get; set; }
    public TagsFilterMode TagsMode { get; set; } = TagsFilterMode.All;

    public string? Status { get; set; } 

    public ServerSortField SortBy { get; set; } = ServerSortField.UpdatedAt;
    public SortDirection SortDir { get; set; } = SortDirection.Desc;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
