using Helios.Infrastructure.ResultProcessing;
using Helios.Platform.Common;
using Helios.Platform.Servers;

namespace Helios.Application.Platform.Servers;

public interface IServersAppService
{
    Task<AppResult<ServerDto>> CreateAsync(CreateServerRequest request, CancellationToken ct);
    Task<AppResult<ServerDto>> GetByIdAsync(Guid serverId, CancellationToken ct);
    Task<AppResult<PagedResponse<ServerDto>>> GetListAsync(GetServersRequest request, CancellationToken ct);
    Task<AppResult<ServerDto>> UpdateAsync(Guid serverId, UpdateServerRequest request, CancellationToken ct);
}
