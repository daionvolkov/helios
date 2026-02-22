using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Helios.Persistence.Enums;
using Helios.Platform.Agents.Intrefaces;
using Helios.Platform.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Security.Cryptography;
using Helios.Infrastructure.ResultProcessing;

namespace Helios.Platform.Agents;

public sealed class AgentManager : IAgentManager
{
    private readonly HeliosDbContext _db;
    private readonly ILogger<AgentManager> _logger;

    public AgentManager(HeliosDbContext db, ILogger<AgentManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DomainResult<(Guid AgentId, string AccessKeyId, string SecretPlain)>> CreateAgentForServerAsync(
        Guid tenantId,
        Guid serverId,
        string displayName,
        string agentVersion,
        string os,
        string arch,
        string? capabilitiesJson,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(agentVersion) ||
            string.IsNullOrWhiteSpace(os) ||
            string.IsNullOrWhiteSpace(arch))
        {
            return DomainResult<(Guid, string, string)>.Failure(new AppError(
                DomainErrorCodes.Agents.ValidationFailed,
                "DisplayName, AgentVersion, Os and Arch are required.",
                ErrorKind.Validation));
        }

        var serverExists = await _db.Servers.AsNoTracking()
            .AnyAsync(s => s.TenantId == tenantId && s.ServerId == serverId, ct);

        if (!serverExists)
        {
            return DomainResult<(Guid, string, string)>.Failure(new AppError(
                DomainErrorCodes.Agents.ServerNotFound,
                "Server not found.",
                ErrorKind.NotFound));
        }

        var now = DateTimeOffset.UtcNow;

        var agent = new Agent
        {
            AgentId = Guid.NewGuid(),
            TenantId = tenantId,
            ServerId = serverId,
            DisplayName = displayName.Trim(),
            AgentVersion = agentVersion.Trim(),
            Os = os.Trim(),
            Arch = arch.Trim(),
            CapabilitiesJson = string.IsNullOrWhiteSpace(capabilitiesJson) ? null : capabilitiesJson.Trim(),
            Status = AgentStatus.Active,
            LastSeenAt = null,
            CreatedAt = now
        };

       
        var accessKeyId = GenerateAccessKeyId();
        var secretPlain = GenerateSecret();

        var credential = new AgentCredential
        {
            TenantId = tenantId,
            AgentId = agent.AgentId,
            AccessKeyId = accessKeyId,
            AccessKeyHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secretPlain)),
            IssuedAt = now,
            RevokedAt = null
        };

        _db.Agents.Add(agent);
        _db.AgentCredentials.Add(credential);

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<(Guid, string, string)>.Success((agent.AgentId, accessKeyId, secretPlain));
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // accessKeyId collision is extremely unlikely; simplest safe approach:
            _logger.LogWarning(ex, "AccessKeyId unique violation. Retrying once.");
            return await RetryOnceAsync(agent, tenantId, now, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create agent failed tenantId={TenantId} serverId={ServerId}", tenantId, serverId);
            return DomainResult<(Guid, string, string)>.Failure(new AppError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
        }
    }


    public async Task<DomainResult<IReadOnlyList<Agent>>> GetAgentsByServerAsync(
    Guid tenantId,
    Guid serverId,
    CancellationToken ct)
    {
        var agents = await _db.Agents.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.ServerId == serverId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return DomainResult<IReadOnlyList<Agent>>.Success(agents);
    }

    public async Task<DomainResult<Agent>> GetAgentByIdAsync(
        Guid tenantId,
        Guid agentId,
        CancellationToken ct)
    {
        var agent = await _db.Agents.AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AgentId == agentId, ct);

        if (agent == null)
        {
            return DomainResult<Agent>.Failure(new AppError(
                DomainErrorCodes.Agents.AgentNotFound,
                "Agent not found.",
                ErrorKind.NotFound));
        }

        return DomainResult<Agent>.Success(agent);
    }


    private async Task<DomainResult<(Guid AgentId, string AccessKeyId, string SecretPlain)>> RetryOnceAsync(
        Agent agent,
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        // Detach any tracked credential for this agent
        var tracked = _db.ChangeTracker.Entries<AgentCredential>()
            .FirstOrDefault(e => e.Entity.AgentId == agent.AgentId);

        if (tracked != null)
            tracked.State = EntityState.Detached;

        var accessKeyId = GenerateAccessKeyId();
        var secretPlain = GenerateSecret();

        var credential = new AgentCredential
        {
            TenantId = tenantId,
            AgentId = agent.AgentId,
            AccessKeyId = accessKeyId,
            AccessKeyHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secretPlain)),
            IssuedAt = now,
            RevokedAt = null
        };

        _db.AgentCredentials.Add(credential);

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<(Guid, string, string)>.Success((agent.AgentId, accessKeyId, secretPlain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry create agent credential failed agentId={AgentId}", agent.AgentId);
            return DomainResult<(Guid, string, string)>.Failure(new AppError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static string GenerateAccessKeyId()
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(16));

    private static string GenerateSecret()
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}