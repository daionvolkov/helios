using Helios.Contracts.Auth;
using Helios.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ILoginService _login;
    public AuthController(ILoginService login) => _login = login;


    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _login.LoginAsync(req, ct);
        if (result == null)
            return Unauthorized("Invalid credentials");

        return Ok(result);
    }

}
