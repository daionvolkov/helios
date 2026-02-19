namespace Helios.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid TenantId { get; }
    Guid? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}
