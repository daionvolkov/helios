using Helios.Api.Common;
using Helios.Application.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;

[ApiController]
[Route("tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantsAppService _tenants;

    public TenantsController(ITenantsAppService tenants) => _tenants = tenants;

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await _tenants.GetMyTenantAsync(ct);
        return result.ToActionResult(this);
    }
}
