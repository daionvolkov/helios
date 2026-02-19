using Helios.Infrastructure.ResultProcessing;
using Helios.Platform.Common;
using Helios.Platform.Servers;
using Helios.Platform.Servers.Models;
using Microsoft.Extensions.Logging;

namespace Helios.Application.Platform.Servers;

public sealed class ServersAppService : IServersAppService
{
    private readonly IServerManager _manager;
    private readonly Helios.Application.Abstractions.ICurrentUserContext _current;
    private readonly ILogger<ServersAppService> _logger;

    public ServersAppService(IServerManager manager, Helios.Application.Abstractions.ICurrentUserContext current, ILogger<ServersAppService> logger)
    {
        _manager = manager;
        _current = current;
        _logger = logger;
    }


    public async Task<AppResult<ServerDto>> CreateAsync(CreateServerRequest request, CancellationToken ct)
    {
        if (!CanWrite())
            return AppResult<ServerDto>.Failure(new AppError(ErrorCodes.Common.Forbidden, "Forbidden", ErrorKind.Forbidden));

        var model = new CreateServerModel
        {
            ProjectId = request.ProjectId,
            EnvironmentId = request.EnvironmentId,
            Name = request.Name,
            Description = request.Description,
            Host = request.Host,
            Tags = request.Tags,
            Status = request.Status?.ToString()
        };

        var res = await _manager.CreateAsync(_current.TenantId, model, ct);
        if (!res.IsSuccess)
            return AppResult<ServerDto>.Failure(MapError(res.Error));

        return AppResult<ServerDto>.Success(ToDto(res.Value!));
    }

    public async Task<AppResult<ServerDto>> GetByIdAsync(Guid serverId, CancellationToken ct)
    {
        if (!CanRead())
            return AppResult<ServerDto>.Failure(new AppError(ErrorCodes.Common.Forbidden, "Forbidden", ErrorKind.Forbidden));

        var res = await _manager.GetByIdAsync(_current.TenantId, serverId, ct);
        if (!res.IsSuccess)
            return AppResult<ServerDto>.Failure(MapError(res.Error));

        return AppResult<ServerDto>.Success(ToDto(res.Value!));
    }

    public async Task<AppResult<PagedResponse<ServerDto>>> GetListAsync(GetServersRequest request, CancellationToken ct)
    {
        if (!CanRead())
            return AppResult<PagedResponse<ServerDto>>.Failure(new AppError(ErrorCodes.Common.Forbidden, "Forbidden", ErrorKind.Forbidden));

        var query = new GetServersQuery
        {
            ProjectId = request.ProjectId,
            EnvironmentId = request.EnvironmentId,
            Search = request.Search,
            Tags = request.Tags,
            TagsMode = request.TagsMode == TagsFilterModeDto.Any ? TagsFilterMode.Any : TagsFilterMode.All,
            Status = request.Status?.ToString(),
            SortBy = request.SortBy switch
            {
                ServerSortFieldDto.Name => ServerSortField.Name,
                ServerSortFieldDto.CreatedAt => ServerSortField.CreatedAt,
                _ => ServerSortField.UpdatedAt
            },
            SortDir = request.SortDir == SortDirectionDto.Asc ? SortDirection.Asc : SortDirection.Desc,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var res = await _manager.GetListAsync(_current.TenantId, query, ct);
        if (!res.IsSuccess)
            return AppResult<PagedResponse<ServerDto>>.Failure(MapError(res.Error));

        var dto = new PagedResponse<ServerDto>
        {
            Items = res.Value!.Items.Select(ToDto).ToList(),
            Page = res.Value.Page,
            PageSize = res.Value.PageSize,
            Total = res.Value.Total,
            TotalPages = res.Value.TotalPages
        };

        return AppResult<PagedResponse<ServerDto>>.Success(dto);
    }

    public async Task<AppResult<ServerDto>> UpdateAsync(Guid serverId, UpdateServerRequest request, CancellationToken ct)
    {
        if (!CanWrite())
            return AppResult<ServerDto>.Failure(new AppError(ErrorCodes.Common.Forbidden, "Forbidden", ErrorKind.Forbidden));

        var model = new UpdateServerModel
        {
            ProjectId = request.ProjectId,
            EnvironmentId = request.EnvironmentId,
            Name = request.Name,
            Description = request.Description,
            Host = request.Host,
            Tags = request.Tags,
            Status = request.Status?.ToString()
        };

        var res = await _manager.UpdateAsync(_current.TenantId, serverId, model, ct);
        if (!res.IsSuccess)
            return AppResult<ServerDto>.Failure(MapError(res.Error));

        return AppResult<ServerDto>.Success(ToDto(res.Value!));
    }

    private bool CanRead() => _current.IsInRole("Owner") || _current.IsInRole("Admin") || _current.IsInRole("Viewer");
    private bool CanWrite() => _current.IsInRole("Owner") || _current.IsInRole("Admin");

    private static AppError MapError(DomainError? e)
    {
        if (e == null)
            return new AppError(ErrorCodes.Common.Unexpected, "Unexpected error", ErrorKind.Internal);

        var kind = e.Kind switch
        {
            DomainErrorKind.Validation => ErrorKind.Validation,
            DomainErrorKind.Unauthorized => ErrorKind.Unauthorized,
            DomainErrorKind.Forbidden => ErrorKind.Forbidden,
            DomainErrorKind.NotFound => ErrorKind.NotFound,
            DomainErrorKind.Conflict => ErrorKind.Conflict,
            _ => ErrorKind.Internal
        };

        return new AppError(e.Code, e.Message, kind);
    }

    private static ServerDto ToDto(Helios.Persistence.Entities.Server s)
    {
        return new ServerDto
        {
            ServerId = s.ServerId,
            TenantId = s.TenantId,
            ProjectId = s.ProjectId,
            EnvironmentId = s.EnvironmentId,
            Name = s.Name,
            Description = s.Description,
            Hostname = s.Hostname,
            // TagsJson = s.TagsJson.Split(','),
            Status = Enum.TryParse<ServerStatus>(s.Status, true, out var st) ? st : ServerStatus.Active,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
