using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ssomero.Api.Configuration;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

/// <summary>
/// Webhook endpoint for payment provider callbacks (Flutterwave, Yo Uganda, etc.).
/// 
/// Responsibilities:
/// 1. Verify webhook signature to ensure authenticity
/// 2. Normalize provider-specific status to internal PaymentStatus
/// 3. Idempotently process duplicate callbacks
/// 4. Activate/extend subscription on payment success
/// 5. Return 200 OK immediately to acknowledge receipt
/// </summary>
[ApiController]
[Route("api/webhooks/payments")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly PaymentSettings _settings;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        IPaymentService payments,
        IOptions<PaymentSettings> settings,
        ILogger<PaymentWebhookController> logger)
    {
        _payments = payments;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/webhooks/payments/flutterwave
    /// 
    /// Accepts Flutterwave payment completion callbacks.
    /// Requires X-Flw-Signature header matching the webhook secret.
    /// </summary>
    [HttpPost("flutterwave")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> HandleFlutterwaveWebhook(CancellationToken ct)
    {
        try
        {
            // Step 1: Verify signature
            if (!Request.Headers.TryGetValue("X-Flw-Signature", out var signatureHeader))
            {
                _logger.LogWarning("Flutterwave webhook: missing X-Flw-Signature header");
                return BadRequest("Missing signature");
            }

            var body = await ReadRequestBodyAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Flutterwave webhook: empty body");
                return BadRequest("Empty body");
            }

            if (!VerifyFlutterwaveSignature(body, signatureHeader.ToString()))
            {
                _logger.LogWarning("Flutterwave webhook: signature verification failed");
                return Unauthorized("Invalid signature");
            }

            // Step 2: Parse webhook payload
            var payload = JsonSerializer.Deserialize<FlutterwaveWebhookPayload>(body);
            if (payload is null)
            {
                _logger.LogWarning("Flutterwave webhook: failed to parse payload");
                return BadRequest("Invalid payload");
            }

            _logger.LogInformation(
                "Flutterwave webhook received: Event={Event}, Status={Status}, TxRef={TxRef}",
                payload.Event, payload.Data?.Status, payload.Data?.TxRef);

            // Step 3: Only process payment completion events
            if (payload.Event != "charge.completed")
            {
                _logger.LogInformation(
                    "Flutterwave webhook: ignoring non-charge.completed event: {Event}", payload.Event);
                return Ok();
            }

            if (payload.Data?.TxRef is null)
            {
                _logger.LogWarning("Flutterwave webhook: missing TxRef in data");
                return BadRequest("Missing TxRef");
            }

            // Step 4: Normalize status and process idempotently
            var status = NormalizeFlutterwaveStatus(payload.Data.Status);
            await _payments.HandleWebhookAsync(payload.Data.TxRef, status, ct);

            // Step 5: Return 200 OK immediately
            return Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Flutterwave webhook: request cancelled");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave webhook processing failed");
            return Ok(); // Return 200 anyway — provider may retry
        }
    }

    /// <summary>
    /// POST /api/webhooks/payments/yo-uganda
    /// 
    /// Accepts Yo Uganda payment completion callbacks.
    /// Requires X-Yo-Signature header or query parameter signature.
    /// </summary>
    [HttpPost("yo-uganda")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> HandleYoUgandaWebhook(CancellationToken ct)
    {
        try
        {
            var body = await ReadRequestBodyAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Yo Uganda webhook: empty body");
                return BadRequest("Empty body");
            }

            // Verify signature (similar to Flutterwave)
            if (!Request.Headers.TryGetValue("X-Yo-Signature", out var signatureHeader))
            {
                _logger.LogWarning("Yo Uganda webhook: missing X-Yo-Signature header");
                return BadRequest("Missing signature");
            }

            if (!VerifyYoUgandaSignature(body, signatureHeader.ToString()))
            {
                _logger.LogWarning("Yo Uganda webhook: signature verification failed");
                return Unauthorized("Invalid signature");
            }

            // Parse payload
            var payload = JsonSerializer.Deserialize<YoUgandaWebhookPayload>(body);
            if (payload is null)
            {
                _logger.LogWarning("Yo Uganda webhook: failed to parse payload");
                return BadRequest("Invalid payload");
            }

            _logger.LogInformation(
                "Yo Uganda webhook received: Status={Status}, ExternalRef={ExternalRef}",
                payload.Status, payload.ExternalRef);

            // Process idempotently
            var status = NormalizeYoUgandaStatus(payload.Status);
            await _payments.HandleWebhookAsync(payload.ExternalRef, status, ct);

            return Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Yo Uganda webhook: request cancelled");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yo Uganda webhook processing failed");
            return Ok();
        }
    }

    // ── Signature Verification ────────────────────────────────────────────

    private bool VerifyFlutterwaveSignature(string body, string signature)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            _logger.LogWarning("Flutterwave webhook secret not configured");
            return false;
        }

        var hash = ComputeSha256Hash(body + _settings.WebhookSecret);
        var valid = hash.Equals(signature, StringComparison.OrdinalIgnoreCase);

        if (!valid)
            _logger.LogWarning("Flutterwave signature mismatch. Expected={Expected}, Got={Got}", hash, signature);

        return valid;
    }

    private bool VerifyYoUgandaSignature(string body, string signature)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            _logger.LogWarning("Yo Uganda webhook secret not configured");
            return false;
        }

        var hash = ComputeSha256Hash(body + _settings.WebhookSecret);
        var valid = hash.Equals(signature, StringComparison.OrdinalIgnoreCase);

        if (!valid)
            _logger.LogWarning("Yo Uganda signature mismatch. Expected={Expected}, Got={Got}", hash, signature);

        return valid;
    }

    private static string ComputeSha256Hash(string input)
    {
        using (var sha = SHA256.Create())
        {
            var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashedBytes);
        }
    }

    // ── Status Normalization ──────────────────────────────────────────────

    /// <summary>
    /// Normalize Flutterwave provider status to internal status.
    /// Flutterwave returns: successful, failed, pending, cancelled, etc.
    /// Internal: Pending, Completed, Failed, Cancelled, Expired, Refunded
    /// </summary>
    private static string NormalizeFlutterwaveStatus(string? providerStatus)
    {
        return providerStatus?.ToLowerInvariant() switch
        {
            "successful" => "successful",
            "failed" => "failed",
            "pending" => "pending",
            "cancelled" => "cancelled",
            "expired" => "expired",
            "refunded" => "refunded",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Normalize Yo Uganda provider status to internal status.
    /// </summary>
    private static string NormalizeYoUgandaStatus(string? providerStatus)
    {
        return providerStatus?.ToLowerInvariant() switch
        {
            "completed" or "success" or "successful" => "successful",
            "failed" => "failed",
            "pending" => "pending",
            "cancelled" => "cancelled",
            "refunded" => "refunded",
            _ => "unknown"
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<string> ReadRequestBodyAsync()
    {
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            return body;
        }
    }

    // ── Webhook Payload Types ─────────────────────────────────────────────

    private sealed record FlutterwaveWebhookPayload(
        [property: System.Text.Json.Serialization.JsonPropertyName("event")]
        string? Event,
        [property: System.Text.Json.Serialization.JsonPropertyName("data")]
        FlutterwaveData? Data
    );

    private sealed record FlutterwaveData(
        [property: System.Text.Json.Serialization.JsonPropertyName("tx_ref")]
        string? TxRef,
        [property: System.Text.Json.Serialization.JsonPropertyName("status")]
        string? Status,
        [property: System.Text.Json.Serialization.JsonPropertyName("amount")]
        decimal Amount = 0,
        [property: System.Text.Json.Serialization.JsonPropertyName("currency")]
        string? Currency = null
    );

    private sealed record YoUgandaWebhookPayload(
        [property: System.Text.Json.Serialization.JsonPropertyName("status")]
        string Status,
        [property: System.Text.Json.Serialization.JsonPropertyName("external_ref")]
        string ExternalRef,
        [property: System.Text.Json.Serialization.JsonPropertyName("amount")]
        decimal Amount = 0
    );
}
