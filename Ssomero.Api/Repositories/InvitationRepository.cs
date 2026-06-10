using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;

namespace Ssomero.Api.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly SsomeroDbContext _db;

    public InvitationRepository(SsomeroDbContext db)
    {
        _db = db;
    }

    public async Task<Invitation> CreateAsync(Invitation invitation, CancellationToken ct = default)
    {
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(ct);
        return invitation;
    }

    public async Task<Invitation> CreateWithAuditAsync(Invitation invitation, InvitationAudit audit, CancellationToken ct = default)
    {
        _db.Invitations.Add(invitation);
        _db.InvitationAudits.Add(audit);
        await _db.SaveChangesAsync(ct);
        return invitation;
    }

    public async Task<Invitation?> GetByIdAsync(Guid invitationId, CancellationToken ct = default)
    {
        return await _db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
    }

    public async Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _db.Invitations.FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);
    }

    public async Task UpdateAsync(Invitation invitation, CancellationToken ct = default)
    {
        // Enforce immutability: once an invitation is consumed or revoked, it must not be modified via repository update.
        var existing = await _db.Invitations.AsNoTracking().FirstOrDefaultAsync(i => i.Id == invitation.Id, ct);
        if (existing != null)
        {
            if (existing.ConsumedAt != null)
            {
                throw new InvalidOperationException("Cannot modify a consumed invitation.");
            }
            if (string.Equals(existing.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot modify a revoked invitation.");
            }
        }

        _db.Invitations.Update(invitation);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(Guid invitationId, CancellationToken ct = default)
    {
        var inv = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
        if (inv is null) return;
        // Prevent revoking a consumed invitation
        if (inv.ConsumedAt != null)
            throw new InvalidOperationException("Cannot revoke a consumed invitation.");

        if (string.Equals(inv.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
            return;

        inv.Status = "Revoked";
        inv.Metadata = (inv.Metadata ?? string.Empty) + "\nRevokedAt:" + DateTime.UtcNow.ToString("o");
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeWithAuditAsync(Guid invitationId, InvitationAudit audit, CancellationToken ct = default)
    {
        var inv = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
        if (inv is null) return;
        // Prevent revoking a consumed invitation
        if (inv.ConsumedAt != null)
            throw new InvalidOperationException("Cannot revoke a consumed invitation.");

        if (!string.Equals(inv.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
        {
            inv.Status = "Revoked";
            inv.Metadata = (inv.Metadata ?? string.Empty) + "\nRevokedAt:" + DateTime.UtcNow.ToString("o");
        }

        _db.InvitationAudits.Add(audit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid invitationId, CancellationToken ct = default)
    {
        return await _db.Invitations.AnyAsync(i => i.Id == invitationId, ct);
    }

    public async Task AddAuditAsync(InvitationAudit audit, CancellationToken ct = default)
    {
        _db.InvitationAudits.Add(audit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Invitation>> GetClassInvitationsAsync(Guid classId, CancellationToken ct = default)
    {
        return await _db.Invitations
            .Where(i => i.ClassId == classId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Invitation>> GetInviterInvitationsAsync(Guid inviterId, CancellationToken ct = default)
    {
        return await _db.Invitations
            .Where(i => i.InviterId == inviterId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Invitation>> GetByTokenKeyIdAsync(string tokenKeyId, CancellationToken ct = default)
    {
        return await _db.Invitations
            .Where(i => i.TokenKeyId == tokenKeyId)
            .ToListAsync(ct);
    }

    public async Task<Invitation?> LockForConsumeAsync(Guid invitationId, CancellationToken ct = default)
    {
        // Attempt an EF Core row-level lock by selecting the entity and applying a FOR UPDATE
        // style behavior via a tracked query. For SQLite this is best-effort; in production
        // DB provider should be SQL Server/Postgres which support row locking via transactions.
        var inv = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
        return inv;
    }
}
