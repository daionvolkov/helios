using Helios.Contracts.Agents.Agents;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Application.Agents;

public interface IAgentsAppService
{
    Task<AppResult<List<AgentDto>>> GetByServerAsync(Guid serverId, CancellationToken ct);
    Task<AppResult<AgentDto>> GetByIdAsync(Guid agentId, CancellationToken ct);
}