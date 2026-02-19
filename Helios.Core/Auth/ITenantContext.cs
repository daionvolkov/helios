namespace Helios.Core.Auth;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
}
