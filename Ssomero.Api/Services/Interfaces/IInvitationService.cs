using System;
using System.Threading;
using System.Threading.Tasks;
using Ssomero.Api.Entities;
using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IInvitationService
{
    Task<CreateInvitationResult> CreateInvitationAsync(Guid inviterId, Guid? classId, string? inviteeContact, Guid? inviteeStudentId, string purpose, DateTime expiresAt, string? metadata = null, string? idempotencyKey = null, CancellationToken ct = default);

    Task<InvitationValidationResult> ValidateInvitationAsync(string token, CancellationToken ct = default);

    Task<bool> RevokeInvitationAsync(Guid invitationId, Guid actorId, string? reason = null, CancellationToken ct = default);

    Task<InvitationDetailsDto?> GetInvitationAsync(Guid invitationId, CancellationToken ct = default);
}
