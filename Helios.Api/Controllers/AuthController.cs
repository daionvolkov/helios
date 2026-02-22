using Helios.Api.Common;
using Helios.Application.Identity;
using Helios.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _auth;
    public AuthController(IAuthAppService auth) => _auth = auth;


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(req, ct);
        return result.ToIActionResult(this);
    }

}
