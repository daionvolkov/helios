using Helios.Api.Common;
using Helios.Application.Agents;
using Helios.Contracts.Agents.Agents;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;
[ApiController]
[Route("agent")]
public class AgentsController(IAgentsAppService app) : ControllerBase
{
    
    [HttpGet("/servers/{serverId:guid}/agents")]
    public async Task<ActionResult<List<AgentDto>>> GetByServer([FromRoute] Guid serverId, CancellationToken ct)
    {
        var res = await app.GetByServerAsync(serverId, ct);
        return res.ToActionResult(this);
    }

    [HttpGet("/agents/{agentId:guid}")]
    public async Task<ActionResult<AgentDto>> GetById([FromRoute] Guid agentId, CancellationToken ct)
    {
        var res = await app.GetByIdAsync(agentId, ct);
        return res.ToActionResult(this);
    }
}