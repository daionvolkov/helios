using Helios.Contracts.Auth;

namespace Helios.Identity.Services;

public interface ILoginService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
}
