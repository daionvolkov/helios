using Helios.Contracts.Agents.Agents;
using Helios.Contracts.Agents.Enrollment;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Application.Agents;

public interface IEnrollmentAppService
{
    Task<AppResult<CreateEnrollmentTokenResponse>> IssueEnrollmentTokenAsync(Guid serverId, CancellationToken ct);

    Task<AppResult<AgentEnrollResponse>> EnrollAsync(AgentEnrollRequest? request, CancellationToken ct);
}