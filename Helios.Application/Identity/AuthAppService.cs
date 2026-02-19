
using Helios.Contracts.Auth;
using Helios.Identity.Services;
using Helios.Infrastructure.ResultProcessing;
using Microsoft.Extensions.Logging;

namespace Helios.Application.Identity;

public sealed class AuthAppService : IAuthAppService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<AuthAppService> _logger;

    public AuthAppService(ILoginService loginService, ILogger<AuthAppService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public async Task<AppResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        try
        {
            var res = await _loginService.LoginAsync(request, ct);
            if (res == null)
            {
                _logger.LogWarning("Login failed for email={Email}", request.Email);
                return AppResult<LoginResponse>.Failure(
                    new AppError(ErrorCodes.Auth.InvalidCredentials, "Invalid credentials", ErrorKind.Unauthorized)
                );
            }

            _logger.LogInformation("Login success for userId={UserId} tenantId={TenantId}", res.User.UserId, res.User.TenantId);
            return AppResult<LoginResponse>.Success(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed unexpectedly for email={Email}", request.Email);
            return AppResult<LoginResponse>.Failure(
                new AppError(ErrorCodes.Common.Unexpected, "Unexpected error", ErrorKind.Internal)
            );
        }
    }
}
