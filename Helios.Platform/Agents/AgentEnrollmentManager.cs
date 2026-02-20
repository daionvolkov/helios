using Helios.Persistence;
using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Helios.Platform.Agents.Intrefaces;
using Helios.Platform.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Helios.Platform.Agents;

public sealed class AgentEnrollmentManager : IAgentEnrollmentManager
{
    private readonly HeliosDbContext _db;
    private readonly ILogger<AgentEnrollmentManager> _logger;

    public AgentEnrollmentManager(HeliosDbContext db, ILogger<AgentEnrollmentManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DomainResult<(string Token, DateTimeOffset ExpiresAt)>> CreateTokenAsync(
        Guid tenantId,
        Guid serverId,
        Guid? createdByUserId,
        TimeSpan ttl,
        CancellationToken ct)
    {
        if (ttl <= TimeSpan.Zero || ttl > TimeSpan.FromDays(7))
        {
            return DomainResult<(string, DateTimeOffset)>.Failure(new DomainError(
                DomainErrorCodes.Agents.ValidationFailed,
                "Invalid TTL.",
                DomainErrorKind.Validation));
        }

        // Ensure server exists and belongs to tenant
        var serverExists = await _db.Servers.AsNoTracking()
            .AnyAsync(s => s.TenantId == tenantId && s.ServerId == serverId, ct);

        if (!serverExists)
        {
            return DomainResult<(string, DateTimeOffset)>.Failure(new DomainError(
                DomainErrorCodes.Agents.ServerNotFound,
                "Server not found.",
                DomainErrorKind.NotFound));
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(ttl);

        var tokenPlain = GenerateToken();
        var tokenHash = Sha256Bytes(tokenPlain);

        var entity = new AgentEnrollmentToken
        {
            TokenId = Guid.NewGuid(),
            TenantId = tenantId,
            ServerId = serverId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            UsedAt = null,
            CreatedBy = createdByUserId,
            CreatedAt = now
        };

        _db.AgentEnrollmentTokens.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<(string, DateTimeOffset)>.Success((tokenPlain, expiresAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create enrollment token failed tenantId={TenantId} serverId={ServerId}", tenantId, serverId);
            return DomainResult<(string, DateTimeOffset)>.Failure(new DomainError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                DomainErrorKind.Internal));
        }
    }

    public async Task<DomainResult<(Guid TenantId, Guid ServerId)>> ValidateAndConsumeAsync(string plainToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
        {
            return DomainResult<(Guid, Guid)>.Failure(new DomainError(
                DomainErrorCodes.Agents.TokenInvalid,
                "Invalid token.",
                DomainErrorKind.Validation));
        }

        var tokenHash = Sha256Bytes(plainToken.Trim());

        // Find token by hash (byte[] comparison must be sequence-equal in query)
        // For PostgreSQL bytea EF translates == to bytea equality, so this is OK.
        var token = await _db.AgentEnrollmentTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (token == null)
        {
            return DomainResult<(Guid, Guid)>.Failure(new DomainError(
                DomainErrorCodes.Agents.TokenInvalid,
                "Invalid token.",
                DomainErrorKind.NotFound));
        }

        var now = DateTimeOffset.UtcNow;

        if (token.UsedAt.HasValue)
        {
            return DomainResult<(Guid, Guid)>.Failure(new DomainError(
                DomainErrorCodes.Agents.TokenAlreadyUsed,
                "Token already used.",
                DomainErrorKind.Conflict));
        }

        if (token.ExpiresAt <= now)
        {
            return DomainResult<(Guid, Guid)>.Failure(new DomainError(
                DomainErrorCodes.Agents.TokenExpired,
                "Token expired.",
                DomainErrorKind.Conflict));
        }

        token.UsedAt = now;

        try
        {
            await _db.SaveChangesAsync(ct);
            return DomainResult<(Guid, Guid)>.Success((token.TenantId, token.ServerId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Consume enrollment token failed tokenId={TokenId}", token.TokenId);
            return DomainResult<(Guid, Guid)>.Failure(new DomainError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                DomainErrorKind.Internal));
        }
    }

    private static string GenerateToken()
    {
        // 32 bytes entropy => strong token; base64url for transport safety
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static byte[] Sha256Bytes(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return SHA256.HashData(bytes);
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}