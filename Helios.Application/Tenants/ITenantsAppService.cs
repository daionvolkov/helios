using Helios.Contracts.Tenants;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Application.Tenants;

public interface ITenantsAppService
{
    Task<AppResult<TenantMeResponse>> GetMyTenantAsync(CancellationToken ct);
}
