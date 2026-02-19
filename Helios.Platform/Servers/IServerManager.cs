using Helios.Persistence.Entities;
using Helios.Platform.Common;
using Helios.Platform.Servers.Models;

namespace Helios.Platform.Servers;

public interface IServerManager
{
    Task<DomainResult<Server>> CreateAsync(Guid tenantId, CreateServerModel model, CancellationToken ct);
    Task<DomainResult<Server>> GetByIdAsync(Guid tenantId, Guid serverId, CancellationToken ct);
    Task<DomainResult<PagedResult<Server>>> GetListAsync(Guid tenantId, GetServersQuery query, CancellationToken ct);
    Task<DomainResult<Server>> UpdateAsync(Guid tenantId, Guid serverId, UpdateServerModel model, CancellationToken ct);
}
