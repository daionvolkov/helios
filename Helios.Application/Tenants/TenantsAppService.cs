using Helios.Application.Abstractions;
using Helios.Contracts.Tenants;
using Helios.Infrastructure.ResultProcessing;
using Helios.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Helios.Application.Tenants;

public sealed class TenantsAppService : ITenantsAppService
{
    private readonly HeliosDbContext _db;
    private readonly ICurrentUserContext _current;
    private readonly ILogger<TenantsAppService> _logger;


    public TenantsAppService(HeliosDbContext db, ICurrentUserContext current, ILogger<TenantsAppService> logger)
    {
        _db = db;
        _current = current;
        _logger = logger;
    }

    public async Task<AppResult<TenantMeResponse>> GetMyTenantAsync(CancellationToken ct)
    {
        try
        {
            var tenantId = _current.TenantId;

            var tenant = await _db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found tenantId={TenantId}", tenantId);
                return AppResult<TenantMeResponse>.Failure(
                    new AppError(ErrorCodes.Tenants.NotFound, "Tenant not found", ErrorKind.NotFound)
                );
            }

            return AppResult<TenantMeResponse>.Success(new TenantMeResponse
            {
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                CreatedAt = tenant.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyTenant failed");
            return AppResult<TenantMeResponse>.Failure(
                new AppError(ErrorCodes.Common.Unexpected, "Unexpected error", ErrorKind.Internal)
            );
        }
    }
}
