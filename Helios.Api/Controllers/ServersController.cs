using Helios.Api.Common;
using Helios.Application.Platform.Servers;
using Helios.Platform.Servers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;

[ApiController]
[Route("servers")]
//[Authorize]

public sealed class ServersController : ControllerBase
{
    private readonly IServersAppService _servers;
    public ServersController(IServersAppService servers) => _servers = servers;

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateServerRequest request, CancellationToken ct)
    {
        var res = await _servers.CreateAsync(request, ct);
        return res.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetServersRequest request, CancellationToken ct)
    {
        var res = await _servers.GetListAsync(request, ct);
        return res.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var res = await _servers.GetByIdAsync(id, ct);
        return res.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateServerRequest request, CancellationToken ct)
    {
        var res = await _servers.UpdateAsync(id, request, ct);
        return res.ToActionResult(this);
    }
}
