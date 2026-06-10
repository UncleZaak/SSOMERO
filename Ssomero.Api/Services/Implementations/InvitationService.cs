using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Repositories.Interfaces;
using Ssomero.Api.Services.Interfaces;
using Ssomero.Api.Entities;
using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Implementations;

public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _repo;
    private readonly ILogger<InvitationService> _logger;
    private readonly Security.IKeyProvider _keyProvider;

    public InvitationService(IInvitationRepository repo, ILogger<InvitationService> logger, Security.IKeyProvider keyProvider)
    {
        _repo = repo;
        _logger = logger;
        _keyProvider = keyProvider;
    }

    public async Task<CreateInvitationResult> CreateInvitationAsync(Guid inviterId, Guid? classId, string? inviteeContact, Guid? inviteeStudentId, string purpose, DateTime expiresAt, string? metadata = null, string? idempotencyKey = null, CancellationToken ct = default)
    {
        // Generate raw token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(tokenBytes).TrimEnd('=');

        // Compute token hash using HMACSHA256 with configured current key
        var tokenKeyId = _keyProvider.GetCurrentKeyId();
        var key = _keyProvider.GetCurrentKey();
        using var hmac = new HMACSHA256(key);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        var tokenHash = Convert.ToBase64String(hashBytes);

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            InviterId = inviterId,
            InviteeStudentId = inviteeStudentId,
            InviteeContact = inviteeContact,
            Purpose = purpose,
            TokenHash = tokenHash,
            TokenKeyId = tokenKeyId,
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            MaxUses = 1,
            SingleUse = true,
            Metadata = metadata
        };

        // Persist invitation and audit together
        // Persist invitation and audit atomically using a DbContext transaction
        // To achieve this without exposing DbContext directly on the repository, we can
        // rely on the repository's DbContext instance if accessible; however current pattern
        // does not expose it. So temporarily inject a transaction via creating both entities
        // and saving them together by using repository.CreateAsync followed by AddAuditAsync
        // which call SaveChanges each. For true atomicity, we will leverage a simple transaction
        // by reusing the repository SaveChanges via internal access. A pragmatic approach here is
        // to create a combined object and call CreateAsync then AddAuditAsync; if AddAuditAsync fails
        // we delete the created invitation to rollback the visible change.
        var audit = new InvitationAudit
        {
            Id = Guid.NewGuid(),
            InvitationId = invitation.Id,
            EventType = "Created",
            ActorId = inviterId,
            Timestamp = DateTime.UtcNow,
            Details = "Invitation created",
            CorrelationId = null
        };

        await _repo.CreateWithAuditAsync(invitation, audit, ct);

        // Return raw token to caller for immediate delivery (caller responsible for secure transport)
        var deliveryHint = $"invite://{invitation.Id}";
        // NOTE: rawToken must NOT be logged or stored.
        return new CreateInvitationResult(invitation.Id, invitation.ExpiresAt, deliveryHint);
    }

    public async Task<InvitationValidationResult> ValidateInvitationAsync(string token, CancellationToken ct = default)
    {
        // Compute hash using all known keys and attempt to find matching invitation
        var keys = _keyProvider.GetAllKeys();
        foreach (var kv in keys)
        {
            var key = kv.Value;
            using var hmac = new HMACSHA256(key);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            var inv = await _repo.GetByTokenHashAsync(Convert.ToBase64String(computed), ct);
            if (inv is null) continue;

            // Perform constant-time comparison of stored hash bytes vs computed bytes
            try
            {
                var storedBytes = Convert.FromBase64String(inv.TokenHash);
                if (!CryptographicOperations.FixedTimeEquals(storedBytes, computed))
                {
                    // Hash mismatch (shouldn't happen since query matched base64 string), continue
                    continue;
                }
            }
            catch
            {
                // If stored TokenHash is invalid base64, skip
                continue;
            }

            // Verify statuses
            if (inv.Status == "Revoked")
            {
                await _repo.AddAuditAsync(new InvitationAudit { Id = Guid.NewGuid(), InvitationId = inv.Id, EventType = "Validated", ActorId = null, Timestamp = DateTime.UtcNow, Details = "Revoked", CorrelationId = null }, ct);
                return new InvitationValidationResult(false, inv.Id, inv.Status, "Revoked");
            }
            if (DateTime.UtcNow > inv.ExpiresAt)
            {
                await _repo.AddAuditAsync(new InvitationAudit { Id = Guid.NewGuid(), InvitationId = inv.Id, EventType = "Validated", ActorId = null, Timestamp = DateTime.UtcNow, Details = "Expired", CorrelationId = null }, ct);
                return new InvitationValidationResult(false, inv.Id, inv.Status, "Expired");
            }
            if (inv.ConsumedAt != null)
            {
                await _repo.AddAuditAsync(new InvitationAudit { Id = Guid.NewGuid(), InvitationId = inv.Id, EventType = "Validated", ActorId = null, Timestamp = DateTime.UtcNow, Details = "AlreadyConsumed", CorrelationId = null }, ct);
                return new InvitationValidationResult(false, inv.Id, inv.Status, "AlreadyConsumed");
            }

            // Matched and valid
            await _repo.AddAuditAsync(new InvitationAudit { Id = Guid.NewGuid(), InvitationId = inv.Id, EventType = "Validated", ActorId = null, Timestamp = DateTime.UtcNow, Details = "Valid", CorrelationId = null }, ct);
            return new InvitationValidationResult(true, inv.Id, inv.Status, null);
        }

        await _repo.AddAuditAsync(new InvitationAudit { Id = Guid.NewGuid(), InvitationId = Guid.Empty, EventType = "Validated", ActorId = null, Timestamp = DateTime.UtcNow, Details = "TokenNotFound", CorrelationId = null }, ct);
        return new InvitationValidationResult(false, null, null, "InvalidToken");
    }

    public async Task<bool> RevokeInvitationAsync(Guid invitationId, Guid actorId, string? reason = null, CancellationToken ct = default)
    {
        // Load full invitation to support idempotent revoke behavior
        var inv = await _repo.GetByIdAsync(invitationId, ct);
        if (inv is null) return false;

        // If already revoked, return true (idempotent)
        if (string.Equals(inv.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
            return true;

        var audit = new InvitationAudit
        {
            Id = Guid.NewGuid(),
            InvitationId = invitationId,
            EventType = "Revoked",
            ActorId = actorId,
            Timestamp = DateTime.UtcNow,
            Details = reason,
            CorrelationId = null
        };

        await _repo.RevokeWithAuditAsync(invitationId, audit, ct);

        return true;
    }

    public async Task<InvitationDetailsDto?> GetInvitationAsync(Guid invitationId, CancellationToken ct = default)
    {
        var inv = await _repo.GetByIdAsync(invitationId, ct);
        if (inv is null) return null;
        return new InvitationDetailsDto(inv.Id, inv.ClassId, inv.InviterId, inv.Purpose, inv.Status, inv.CreatedAt, inv.ExpiresAt, inv.InviteeStudentId, inv.InviteeContact, inv.Metadata);
    }
}
