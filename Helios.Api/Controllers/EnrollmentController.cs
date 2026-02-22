using Helios.Api.Common;
using Helios.Application.Agents;
using Helios.Contracts.Agents.Agents;
using Helios.Contracts.Agents.Enrollment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;

[ApiController]
[Route("enrollment")]
public class EnrollmentController(IEnrollmentAppService app) : ControllerBase
{
    private readonly IEnrollmentAppService _app = app;
    
    
    [HttpPost("/servers/{serverId:guid}/enrollment-tokens")]
    [Authorize] 
    public async Task<ActionResult<CreateEnrollmentTokenResponse>> Issue([FromRoute] Guid serverId, CancellationToken ct)
    {
        var result = await _app.IssueEnrollmentTokenAsync(serverId, ct);
        return result.ToActionResult(this);
    }

    // agent-side
    [HttpPost("/agents/enroll")]
    [AllowAnonymous]
    public async Task<ActionResult<CreateEnrollmentTokenResponse>> Enroll([FromBody] AgentEnrollRequest request, CancellationToken ct)
    {
        var result = await _app.EnrollAsync(request, ct);
        return result.ToActionResult(this);
    }
}