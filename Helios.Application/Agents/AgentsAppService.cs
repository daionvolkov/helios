using Helios.Application.Abstractions;
using Helios.Contracts.Agents.Agents;
using Helios.Infrastructure.ResultProcessing;
using Helios.Platform.Agents.Intrefaces;

namespace Helios.Application.Agents;

public sealed class AgentsAppService : IAgentsAppService
{
    private readonly ICurrentUserContext _current;
    private readonly IAgentManager _agents;

    public AgentsAppService(ICurrentUserContext current, IAgentManager agents)
    {
        _current = current;
        _agents = agents;
    }
    
    public async Task<AppResult<List<AgentDto>>> GetByServerAsync(Guid serverId, CancellationToken ct)
    {
        // RBAC: Owner/Admin/Viewer
        if (!CanRead())
            return AppResult<List<AgentDto>>.Forbidden();

        var tenantId = _current.TenantId;
        var res = await _agents.GetAgentsByServerAsync(tenantId, serverId, ct);
        if (!res.IsSuccess)
            return AppResult<List<AgentDto>>.Failure(res.Error!.ToAppError());

        var dto = res.Value.Select(ToDto).ToList();
        return AppResult<List<AgentDto>>.Success(dto);
    }

    public async Task<AppResult<AgentDto>> GetByIdAsync(Guid agentId, CancellationToken ct)
    {
        if (!CanRead())
            return AppResult<AgentDto>.Forbidden();

        var tenantId = _current.TenantId;
        var res = await _agents.GetAgentByIdAsync(tenantId, agentId, ct);
        if (!res.IsSuccess)
            return AppResult<AgentDto>.Failure(res.Error!.ToAppError());

        return AppResult<AgentDto>.Success(ToDto(res.Value));
    }

    private bool CanRead()
        => _current.IsInRole("Owner") || _current.IsInRole("Administrator") || _current.IsInRole("Admin") || _current.IsInRole("Viewer");

    private static AgentDto ToDto(Helios.Persistence.Entities.Agent a)
        => new()
        {
            AgentId = a.AgentId,
            TenantId = a.TenantId,
            ServerId = a.ServerId,
            Name = a.DisplayName,                
            Status = a.Status.ToString(),
            LastSeenAt = a.LastSeenAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.CreatedAt              
        };
}