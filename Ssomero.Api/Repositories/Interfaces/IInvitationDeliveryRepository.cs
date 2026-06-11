using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Repositories.Interfaces;

public interface IInvitationDeliveryRepository
{
    Task<InvitationDelivery> CreateAsync(InvitationDelivery delivery, CancellationToken ct = default);
    Task UpdateAsync(InvitationDelivery delivery, CancellationToken ct = default);
    Task<InvitationDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAuditAsync(InvitationDeliveryAudit audit, CancellationToken ct = default);
}
