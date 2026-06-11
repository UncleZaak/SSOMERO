using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;

namespace Ssomero.Api.Repositories;

public class InvitationDeliveryRepository : IInvitationDeliveryRepository
{
    private readonly SsomeroDbContext _db;

    public InvitationDeliveryRepository(SsomeroDbContext db)
    {
        _db = db;
    }

    public async Task<InvitationDelivery> CreateAsync(InvitationDelivery delivery, CancellationToken ct = default)
    {
        delivery.QueuedAt = DateTime.UtcNow;
        _db.InvitationDeliveries.Add(delivery);
        await _db.SaveChangesAsync(ct);
        return delivery;
    }

    public async Task UpdateAsync(InvitationDelivery delivery, CancellationToken ct = default)
    {
        _db.InvitationDeliveries.Update(delivery);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<InvitationDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.InvitationDeliveries.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddAuditAsync(InvitationDeliveryAudit audit, CancellationToken ct = default)
    {
        _db.InvitationDeliveryAudits.Add(audit);
        await _db.SaveChangesAsync(ct);
    }
}
