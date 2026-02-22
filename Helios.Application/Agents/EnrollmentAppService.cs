using Helios.Application.Abstractions;
using Helios.Contracts.Agents.Agents;
using Helios.Contracts.Agents.Enrollment;
using Helios.Infrastructure.ResultProcessing;
using Helios.Platform.Agents.Intrefaces;
using Microsoft.Extensions.Logging;

namespace Helios.Application.Agents;

public sealed class EnrollmentAppService : IEnrollmentAppService
{
    private readonly ICurrentUserContext _current;
    private readonly IAgentEnrollmentManager _enrollment;
    private readonly IAgentManager _agents;
    private readonly ILogger<EnrollmentAppService> _logger;

    public EnrollmentAppService(
        ICurrentUserContext current,
        IAgentEnrollmentManager enrollment,
        IAgentManager agents,
        ILogger<EnrollmentAppService> logger)
    {
        _current = current;
        _enrollment = enrollment;
        _agents = agents;
        _logger = logger;
    }

    public async Task<AppResult<CreateEnrollmentTokenResponse>> IssueEnrollmentTokenAsync(Guid serverId, CancellationToken ct)
    {
        if (!_current.IsInRole("Owner") && !_current.IsInRole("Administrator") && !_current.IsInRole("Admin"))
            return AppResult<CreateEnrollmentTokenResponse>.Forbidden(
                message: "Forbidden",
                code: ErrorCodes.Common.Forbidden);

        try
        {
            var tenantId = _current.TenantId;
            var createdBy = _current.UserId;

            var res = await _enrollment.CreateTokenAsync(
                tenantId: tenantId,
                serverId: serverId,
                createdByUserId: createdBy,
                ttl: TimeSpan.FromMinutes(10),
                ct: ct);

            if (!res.IsSuccess)
                return AppResult<CreateEnrollmentTokenResponse>.Failure(res.Error!);

            var (token, expiresAt) = res.Value;

            return AppResult<CreateEnrollmentTokenResponse>.Success(new CreateEnrollmentTokenResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return AppResult<CreateEnrollmentTokenResponse>.Internal(
                message: "Request was cancelled",
                code: ErrorCodes.Common.Cancelled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "IssueEnrollmentTokenAsync failed. serverId={ServerId}, tenantId={TenantId}, userId={UserId}",
                serverId, _current.TenantId, _current.UserId);

            return AppResult<CreateEnrollmentTokenResponse>.Internal(
                message: "Unexpected error while issuing enrollment token",
                code: ErrorCodes.Common.Unexpected);
        }
    }

    public async Task<AppResult<AgentEnrollResponse>> EnrollAsync(AgentEnrollRequest? request, CancellationToken ct)
    {
        if (request is null)
            return AppResult<AgentEnrollResponse>.Validation(
                message: "Request is required",
                code: ErrorCodes.Common.ValidationFailed);

        if (string.IsNullOrWhiteSpace(request.Token))
            return AppResult<AgentEnrollResponse>.Validation(
                message: "Token is required",
                code: ErrorCodes.Common.ValidationFailed);

        try
        {
            var tokenRes = await _enrollment.ValidateAndConsumeAsync(request.Token, ct);
            if (!tokenRes.IsSuccess)
                return AppResult<AgentEnrollResponse>.Failure(tokenRes.Error!);

            var (tenantId, serverId) = tokenRes.Value;

            var createRes = await _agents.CreateAgentForServerAsync(
                tenantId: tenantId,
                serverId: serverId,
                displayName: request.DisplayName,
                agentVersion: request.AgentVersion,
                os: request.Os,
                arch: request.Arch,
                capabilitiesJson: request.CapabilitiesJson,
                ct: ct);

            if (!createRes.IsSuccess)
                return AppResult<AgentEnrollResponse>.Failure(createRes.Error!);

            var (agentId, accessKeyId, secretPlain) = createRes.Value;

            return AppResult<AgentEnrollResponse>.Success(new AgentEnrollResponse
            {
                AgentId = agentId,
                AccessKeyId = accessKeyId,
                Secret = secretPlain,
                IssuedAt = DateTimeOffset.UtcNow
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return AppResult<AgentEnrollResponse>.Internal(
                message: "Request was cancelled",
                code: ErrorCodes.Common.Cancelled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "EnrollAsync failed. tokenPrefix={TokenPrefix}",
                SafeTokenPrefix(request.Token));

            return AppResult<AgentEnrollResponse>.Internal(
                message: "Unexpected error while enrolling agent",
                code: ErrorCodes.Common.Unexpected);
        }

        static string SafeTokenPrefix(string token)
            => string.IsNullOrEmpty(token) ? "" : (token.Length <= 8 ? token : token[..8]);
    }
}