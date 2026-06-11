using Hangfire;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;
using Hangfire.Common;
using Hangfire.Client;

namespace Ssomero.Api.Services;

public class InvitationEmailQueueService
{
    private readonly IInvitationDeliveryRepository _deliveryRepo;
    private readonly ILogger<InvitationEmailQueueService> _logger;
    private readonly IJobClient _jobClient;

    public InvitationEmailQueueService(IInvitationDeliveryRepository deliveryRepo, IJobClient jobClient, ILogger<InvitationEmailQueueService> logger)
    {
        _deliveryRepo = deliveryRepo;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async Task<Guid> QueueInvitationEmailAsync(Guid invitationId, string recipient, string subject, string bodyHtml)
    {
        var delivery = new InvitationDelivery
        {
            InvitationId = invitationId,
            Recipient = recipient,
            Subject = subject,
            BodyHtml = bodyHtml,
            Status = "Queued",
            QueuedAt = DateTime.UtcNow
        };

        await _deliveryRepo.CreateAsync(delivery);

        // Enqueue background job to process the delivery via injected client
        _jobClient.Enqueue<Jobs.InvitationEmailJob>(j => j.ExecuteAsync(delivery.Id));

        await _deliveryRepo.AddAuditAsync(new InvitationDeliveryAudit
        {
            DeliveryId = delivery.Id,
            EventType = "Enqueued",
            Timestamp = DateTime.UtcNow,
            Details = "Queued for delivery"
        });

        _logger.LogInformation("Queued invitation email {DeliveryId} to {Recipient}", delivery.Id, recipient);

        return delivery.Id;
    }

    public Task QueueReminderEmailAsync(Guid invitationId, string recipient, string subject, string bodyHtml)
    {
        // For now same as QueueInvitationEmailAsync — could schedule via BackgroundJob.Schedule
        return Task.Run(() => QueueInvitationEmailAsync(invitationId, recipient, subject, bodyHtml));
    }

    public Task QueueExpirationWarningAsync(Guid invitationId, string recipient, string subject, string bodyHtml)
    {
        // For now same as QueueInvitationEmailAsync
        return Task.Run(() => QueueInvitationEmailAsync(invitationId, recipient, subject, bodyHtml));
    }
}
