namespace Helios.Platform.Common;

public sealed class PagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required long Total { get; set; }
    public required int TotalPages { get; set; }
}
