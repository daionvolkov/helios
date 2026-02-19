

namespace Helios.Platform.Servers;

public sealed class GetServersRequest
{
    public Guid? ProjectId { get; set; }
    public Guid? EnvironmentId { get; set; }

    public string? Search { get; set; }

    public List<string>? Tags { get; set; }
    public TagsFilterModeDto TagsMode { get; set; } = TagsFilterModeDto.All;

    public ServerStatus? Status { get; set; }

    public ServerSortFieldDto SortBy { get; set; } = ServerSortFieldDto.UpdatedAt;
    public SortDirectionDto SortDir { get; set; } = SortDirectionDto.Desc;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
