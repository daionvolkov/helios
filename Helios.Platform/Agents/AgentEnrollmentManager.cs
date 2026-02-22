using Helios.Persistence;
using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Helios.Platform.Agents.Intrefaces;
using Helios.Platform.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Helios.Infrastructure.ResultProcessing;

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
            return DomainResult<(string, DateTimeOffset)>.Failure(new AppError(
                DomainErrorCodes.Agents.ValidationFailed,
                "Invalid TTL.",
                ErrorKind.Validation));
        }

        // Ensure server exists and belongs to tenant
        var serverExists = await _db.Servers.AsNoTracking()
            .AnyAsync(s => s.TenantId == tenantId && s.ServerId == serverId, ct);

        if (!serverExists)
        {
            return DomainResult<(string, DateTimeOffset)>.Failure(new AppError(
                DomainErrorCodes.Agents.ServerNotFound,
                "Server not found.",
                ErrorKind.NotFound));
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
            return DomainResult<(string, DateTimeOffset)>.Failure(new AppError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
        }
    }

    public async Task<DomainResult<(Guid TenantId, Guid ServerId)>> ValidateAndConsumeAsync(string plainToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
        {
            return DomainResult<(Guid, Guid)>.Failure(new AppError(
                DomainErrorCodes.Agents.TokenInvalid,
                "Invalid token.",
                ErrorKind.Validation));
        }

        var tokenHash = Sha256Bytes(plainToken.Trim());

        // Find token by hash (byte[] comparison must be sequence-equal in query)
        // For PostgreSQL bytea EF translates == to bytea equality, so this is OK.
        var token = await _db.AgentEnrollmentTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (token == null)
        {
            return DomainResult<(Guid, Guid)>.Failure(new AppError(
                DomainErrorCodes.Agents.TokenInvalid,
                "Invalid token.",
                ErrorKind.NotFound));
        }

        var now = DateTimeOffset.UtcNow;

        if (token.UsedAt.HasValue)
        {
            return DomainResult<(Guid, Guid)>.Failure(new AppError(
                DomainErrorCodes.Agents.TokenAlreadyUsed,
                "Token already used.",
                ErrorKind.Conflict));
        }

        if (token.ExpiresAt <= now)
        {
            return DomainResult<(Guid, Guid)>.Failure(new AppError(
                DomainErrorCodes.Agents.TokenExpired,
                "Token expired.",
                ErrorKind.Conflict));
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
            return DomainResult<(Guid, Guid)>.Failure(new AppError(
                DomainErrorCodes.Agents.Unexpected,
                "Unexpected error.",
                ErrorKind.Internal));
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