
using Helios.Contracts.Auth;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Application.Identity;

public interface IAuthAppService
{
    Task<AppResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}
