using Helios.Core.Auth;

namespace Helios.Application.Abstractions;

public sealed class CurrentUserContextAdapter : ICurrentUserContext
{
    private readonly ITenantContext _tenant;

    public CurrentUserContextAdapter(ITenantContext tenant) => _tenant = tenant;

    public Guid TenantId => _tenant.TenantId;
    public Guid? UserId => _tenant.UserId;
    public IReadOnlyCollection<string> Roles => _tenant.Roles;

    public bool IsInRole(string role) =>
        Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
}
