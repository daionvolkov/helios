using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Helios.Platform.Common;
using Helios.Platform.Servers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Linq.Expressions;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Platform.Servers;

public sealed class ServerManager : IServerManager
{
    private readonly HeliosDbContext _db;
    private readonly ILogger<ServerManager> _logger;

    public ServerManager(HeliosDbContext db, ILogger<ServerManager> logger)
    {
        _db = db;
        _logger = logger;
    }


    public async Task<DomainResult<Server>> CreateAsync(Guid tenantId, CreateServerModel model, CancellationToken ct)
    {
        AppError validationError = ValidateCreate(model);
        return DomainResult<Server>.Failure(validationError);

        var name = model.Name.Trim();
        var normalizedTags = NormalizeTags(model.Tags);

        var exists = await _db.Servers.AsNoTracking()
            .AnyAsync(s => s.TenantId == tenantId && s.Name.ToLower() == name.ToLower(), ct);

        if (exists)
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.NameConflict,
                "Server name already exists.",
                ErrorKind.Conflict));

        var now = DateTimeOffset.UtcNow;

        var server = new Server
        {
            ServerId = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = model.ProjectId,
            EnvironmentId = model.EnvironmentId,
            Name = name,
            Description = model.Description?.Trim(),
            Hostname = model.Host?.Trim(),
            TagsJson = string.Join(", ", normalizedTags),
            Status = NormalizeStatus(model.Status) ?? "Active",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Servers.Add(server);

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<Server>.Success(server);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.NameConflict,
                "Server name already exists.",
                ErrorKind.Conflict));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create server failed tenantId={TenantId} name={Name}", tenantId, name);
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
        }
    }

    public async Task<DomainResult<Server>> GetByIdAsync(Guid tenantId, Guid serverId, CancellationToken ct)
    {
        var server = await _db.Servers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ServerId == serverId, ct);

        if (server == null)
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.NotFound,
                "Server not found.",
                ErrorKind.NotFound));

        return DomainResult<Server>.Success(server);
    }

    public async Task<DomainResult<PagedResult<Server>>> GetListAsync(Guid tenantId, GetServersQuery query, CancellationToken ct)
    {
        var validationError = ValidateList(query);
        if (validationError != null)
            return DomainResult<PagedResult<Server>>.Failure(validationError);

        IQueryable<Server> q = _db.Servers.AsNoTracking().Where(s => s.TenantId == tenantId);

        if (query.ProjectId.HasValue)
            q = q.Where(s => s.ProjectId == query.ProjectId);

        if (query.EnvironmentId.HasValue)
            q = q.Where(s => s.EnvironmentId == query.EnvironmentId);

        var status = NormalizeStatus(query.Status);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(s => s.Status == status);

        var search = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowered = search.ToLower();
            q = q.Where(s =>
                s.Name.ToLower().Contains(lowered) ||
                (s.Hostname != null && s.Hostname.ToLower().Contains(lowered)) ||
                (s.Description != null && s.Description.ToLower().Contains(lowered)));
        }

        var tags = NormalizeTags(query.Tags);
        if (tags.Length > 0)
        {
            if (query.TagsMode == TagsFilterMode.All)
            {
                foreach (var tag in tags)
                    q = q.Where(s => s.TagsJson.Contains(tag)); 
            }
            else
            {
                var predicate = BuildAnyTagPredicate(tags);
                q = q.Where(predicate);
            }
        }

        q = ApplySorting(q, query.SortBy, query.SortDir);

        var total = await q.LongCountAsync(ct);

        var page = query.Page;
        var pageSize = query.PageSize;

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return DomainResult<PagedResult<Server>>.Success(new PagedResult<Server>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = totalPages
        });
    }

    public async Task<DomainResult<Server>> UpdateAsync(Guid tenantId, Guid serverId, UpdateServerModel model, CancellationToken ct)
    {
        var validationError = ValidateUpdate(model);
        if (validationError != null)
            return DomainResult<Server>.Failure(validationError);

        var server = await _db.Servers
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ServerId == serverId, ct);

        if (server == null)
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.NotFound,
                "Server not found.",
                ErrorKind.NotFound));

        if (model.ProjectId.HasValue)
            server.ProjectId = model.ProjectId;

        if (model.EnvironmentId.HasValue)
            server.EnvironmentId = model.EnvironmentId;

        if (model.Name != null)
            server.Name = model.Name.Trim();

        if (model.Description != null)
            server.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        if (model.Host != null)
            server.Hostname = string.IsNullOrWhiteSpace(model.Host) ? null : model.Host.Trim();

        if (model.Tags != null)
        {
            var tagsString = string.Join(", ", NormalizeTags(model.Tags));
            server.TagsJson = tagsString;
        }
            

        if (model.Status != null)
        {
            var st = NormalizeStatus(model.Status);
            if (st == null)
            {
                return DomainResult<Server>.Failure(new AppError(
                    DomainErrorCodes.PlatformServers.ValidationFailed,
                    "Invalid status. Allowed: Active, Inactive.",
                    ErrorKind.Validation));
            }
            server.Status = st;
        }

        server.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<Server>.Success(server);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.NameConflict,
                "Server name already exists.",
                ErrorKind.Conflict));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update server failed tenantId={TenantId} serverId={ServerId}", tenantId, serverId);
            return DomainResult<Server>.Failure(new AppError(
                DomainErrorCodes.PlatformServers.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
        }
    }

    private static AppError ValidateCreate(CreateServerModel model)
    {
        if (model.Name == null)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Name is required.", ErrorKind.Validation);

        var name = model.Name.Trim();
        if (name.Length < 2 || name.Length > 100)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Name length must be 2..100.", ErrorKind.Validation);

        if (model.Tags != null && model.Tags.Count > 20)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Tags max count is 20.", ErrorKind.Validation);

        return null;
    }

    private static AppError? ValidateUpdate(UpdateServerModel model)
    {
        if (model.Name != null)
        {
            var name = model.Name.Trim();
            if (name.Length < 2 || name.Length > 100)
                return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Name length must be 2..100.", ErrorKind.Validation);
        }

        if (model.Tags != null && model.Tags.Count > 20)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Tags max count is 20.", ErrorKind.Validation);

        if (model.Status != null && NormalizeStatus(model.Status) == null)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Invalid status. Allowed: Active, Inactive.", ErrorKind.Validation);

        return null;
    }

    private static AppError? ValidateList(GetServersQuery query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 1;
        if (query.PageSize > 200) query.PageSize = 200;

        if (query.Tags != null && query.Tags.Count > 50)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Too many tags in filter.", ErrorKind.Validation);

        if (query.Status != null && NormalizeStatus(query.Status) == null)
            return new AppError(DomainErrorCodes.PlatformServers.ValidationFailed, "Invalid status. Allowed: Active, Inactive.", ErrorKind.Validation);

        return null;
    }

    private static IQueryable<Server> ApplySorting(IQueryable<Server> q, ServerSortField sortBy, SortDirection sortDir)
    {
        var desc = sortDir == SortDirection.Desc;

        return (sortBy, desc) switch
        {
            (ServerSortField.Name, false) => q.OrderBy(x => x.Name),
            (ServerSortField.Name, true) => q.OrderByDescending(x => x.Name),

            (ServerSortField.CreatedAt, false) => q.OrderBy(x => x.CreatedAt),
            (ServerSortField.CreatedAt, true) => q.OrderByDescending(x => x.CreatedAt),

            (ServerSortField.UpdatedAt, false) => q.OrderBy(x => x.UpdatedAt),
            _ => q.OrderByDescending(x => x.UpdatedAt),
        };
    }

    private static Expression<Func<Server, bool>> BuildAnyTagPredicate(string[] tags)
    {
        var param = Expression.Parameter(typeof(Server), "s");
        var tagsProp = Expression.Property(param, nameof(Server.TagsJson));

        Expression? body = null;
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));

        foreach (var tag in tags)
        {
            var tagConst = Expression.Constant(tag, typeof(string));
            var call = Expression.Call(containsMethod, tagsProp, tagConst);

            body = body == null ? call : Expression.OrElse(body, call);
        }

        body ??= Expression.Constant(true);

        return Expression.Lambda<Func<Server, bool>>(body, param);
    }

    private static string[] NormalizeTags(List<string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return Array.Empty<string>();

        return tags
            .Select(t => t?.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t!.ToLowerInvariant())
            .Distinct()
            .Take(50)
            .ToArray();
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        var s = status.Trim();

        if (string.Equals(s, "Active", StringComparison.OrdinalIgnoreCase))
            return "Active";

        if (string.Equals(s, "Inactive", StringComparison.OrdinalIgnoreCase))
            return "Inactive";

        return null;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}

