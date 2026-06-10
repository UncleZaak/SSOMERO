using System;

namespace Ssomero.Api.Dtos;

public record CreateInvitationResult(Guid InvitationId, DateTime ExpiresAt, string DeliveryHint);

public record InvitationValidationResult(bool IsValid, Guid? InvitationId, string? Status, string? Reason);

public record InvitationDetailsDto(Guid Id, Guid? ClassId, Guid InviterId, string Purpose, string Status, DateTime CreatedAt, DateTime ExpiresAt, Guid? InviteeStudentId, string? InviteeContact, string? Metadata);
