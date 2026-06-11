using Hangfire;
using Microsoft.Extensions.Logging;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;
using Ssomero.Api.Services;

namespace Ssomero.Api.Jobs;

public class InvitationEmailJob
{
    private readonly EmailService _emailService;
    private readonly IInvitationDeliveryRepository _deliveryRepo;
    private readonly ILogger<InvitationEmailJob> _logger;

    public InvitationEmailJob(EmailService emailService, IInvitationDeliveryRepository deliveryRepo, ILogger<InvitationEmailJob> logger)
    {
        _emailService = emailService;
        _deliveryRepo = deliveryRepo;
        _logger = logger;
    }

    // This method is executed by Hangfire worker. Keep the signature simple for serialization.
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[] { 60, 120, 300, 900, 3600 })]
    public async Task ExecuteAsync(Guid deliveryId)
    {
        var delivery = await _deliveryRepo.GetByIdAsync(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Invitation delivery {DeliveryId} not found", deliveryId);
            return;
        }

        // Mark processing
        delivery.Status = "Processing";
        delivery.ProcessingAt = DateTime.UtcNow;
        await _deliveryRepo.UpdateAsync(delivery);

        try
        {
            // Use HTML body if provided
            var body = delivery.BodyHtml ?? string.Empty;
            await _emailService.SendEmailAsync(delivery.Recipient, delivery.Subject, body);

            delivery.Status = "Sent";
            delivery.SentAt = DateTime.UtcNow;
            await _deliveryRepo.UpdateAsync(delivery);

            await _deliveryRepo.AddAuditAsync(new InvitationDeliveryAudit
            {
                DeliveryId = delivery.Id,
                EventType = "Sent",
                Timestamp = DateTime.UtcNow,
                Details = "Email delivered"
            });

            _logger.LogInformation("Invitation email delivered: {DeliveryId}", delivery.Id);
        }
        catch (Exception ex)
        {
            delivery.RetryCount += 1;
            delivery.FailedAt = DateTime.UtcNow;
            delivery.FailureReason = ex.Message;
            delivery.Status = delivery.RetryCount > 5 ? "Failed" : "Retrying";
            await _deliveryRepo.UpdateAsync(delivery);

            await _deliveryRepo.AddAuditAsync(new InvitationDeliveryAudit
            {
                DeliveryId = delivery.Id,
                EventType = "Failed",
                Timestamp = DateTime.UtcNow,
                Details = ex.ToString()
            });

            _logger.LogError(ex, "Failed to deliver invitation email {DeliveryId}", delivery.Id);

            // Rethrow to let Hangfire handle retry/backoff
            throw;
        }
    }
}
