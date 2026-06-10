using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Repositories.Interfaces;

public interface IInvitationRepository
{
    Task<Invitation> CreateAsync(Invitation invitation, CancellationToken ct = default);
    Task<Invitation> CreateWithAuditAsync(Invitation invitation, InvitationAudit audit, CancellationToken ct = default);
    Task<Invitation?> GetByIdAsync(Guid invitationId, CancellationToken ct = default);
    Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task UpdateAsync(Invitation invitation, CancellationToken ct = default);
    Task RevokeAsync(Guid invitationId, CancellationToken ct = default);
    Task RevokeWithAuditAsync(Guid invitationId, InvitationAudit audit, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid invitationId, CancellationToken ct = default);
    Task AddAuditAsync(InvitationAudit audit, CancellationToken ct = default);
    Task<List<Invitation>> GetClassInvitationsAsync(Guid classId, CancellationToken ct = default);
    Task<List<Invitation>> GetInviterInvitationsAsync(Guid inviterId, CancellationToken ct = default);

    Task<List<Invitation>> GetByTokenKeyIdAsync(string tokenKeyId, CancellationToken ct = default);

    /// <summary>
    /// Loads invitation for subsequent consume logic. Caller should update within a transaction
    /// or use optimistic concurrency via RowVersion.
    /// </summary>
    Task<Invitation?> LockForConsumeAsync(Guid invitationId, CancellationToken ct = default);
}
