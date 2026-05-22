using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ssomero.Api.Configuration;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly IPaymentReconciliationService _reconciliation;
    private readonly PaymentSettings _settings;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService payments,
        IPaymentReconciliationService reconciliation,
        IOptions<PaymentSettings> settings,
        ILogger<PaymentsController> logger)
    {
        _payments      = payments;
        _reconciliation = reconciliation;
        _settings      = settings.Value;
        _logger        = logger;
    }

    private Guid GetStudentId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── GET /api/payments/current ────────────────────────────────────────────
    /// <summary>Returns the student's active subscription and latest payment.</summary>
    [HttpGet("current")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var studentId = GetStudentId();
        var subscription  = await _payments.GetActiveSubscriptionAsync(studentId, ct);
        var latestPayment = await _payments.GetLatestPaymentAsync(studentId, ct);
        return Ok(new CurrentPlanResponse(subscription, latestPayment));
    }

    // ── POST /api/payments/initiate ──────────────────────────────────────────
    /// <summary>
    /// Creates a pending payment record and triggers a Mobile Money prompt.
    /// Client must call /verify once the user approves on their handset.
    /// </summary>
    [HttpPost("initiate")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var (success, error, txRef) = await _payments.InitiatePaymentAsync(
            GetStudentId(), req.Plan, req.PhoneNumber, ct);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { txRef, message = "Payment initiated. Approve the Mobile Money prompt on your phone." });
    }

    // ── POST /api/payments/verify ────────────────────────────────────────────
    /// <summary>
    /// Verifies the transaction server-side and activates the subscription on success.
    /// The client MUST NOT be trusted to report payment success — this endpoint
    /// performs the authoritative server-to-provider check.
    /// </summary>
    [HttpPost("verify")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var (success, error) = await _payments.VerifyAndActivateAsync(GetStudentId(), req.TxRef, ct);

        if (!success)
            return BadRequest(new { error });

        var subscription = await _payments.GetActiveSubscriptionAsync(GetStudentId(), ct);
        return Ok(new { message = "Payment verified. Subscription activated.", subscription });
    }

    // ── GET /api/payments/history ────────────────────────────────────────────
    /// <summary>Returns the authenticated student's payment history (up to 20 records).</summary>
    [HttpGet("history")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> GetHistory(CancellationToken ct, [FromQuery] int limit = 20)
    {
        if (limit is < 1 or > 100) limit = 20;
        var history = await _payments.GetPaymentHistoryAsync(GetStudentId(), limit, ct);
        return Ok(history);
    }

    // ── GET /api/payments/{txRef}/status ─────────────────────────────────────
    /// <summary>
    /// Polls the current status of a payment by transaction reference.
    /// Used by the MAUI client after payment initiation to detect completion.
    /// </summary>
    [HttpGet("{txRef}/status")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> GetPaymentStatus(string txRef, CancellationToken ct)
    {
        var studentId = GetStudentId();
        var payment = await _payments.GetPaymentByReferenceAsync(studentId, txRef, ct);

        if (payment is null)
            return NotFound(new { error = "Payment not found." });

        return Ok(payment);
    }

    // ── POST /api/payments/reconcile ─────────────────────────────────────────
    /// <summary>
    /// Reconciles any pending payments for the authenticated student by re-verifying
    /// them with the payment provider. Called automatically by the mobile app on resume.
    /// Safe to call repeatedly — fully idempotent.
    /// </summary>
    [HttpPost("reconcile")]
    [Authorize(Roles = "Student,ClassRepresentative")]
    public async Task<IActionResult> Reconcile(CancellationToken ct)
    {
        var result = await _reconciliation.ReconcilePendingPaymentsAsync(GetStudentId(), ct);
        return Ok(new ReconcileResponse(result.Recovered, result.StillPending, result.Total));
    }

    // ── POST /api/payments/webhook ───────────────────────────────────────────
    /// <summary>
    /// Flutterwave webhook endpoint. Validates the signature header before processing.
    /// Not authenticated via JWT — the provider calls this directly.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        // ── Signature verification ───────────────────────────────────────────
        // Skip signature check when using mock provider or no webhook secret is configured.
        if (!_settings.UseMock && !string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            var signature = Request.Headers["verif-hash"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signature) || signature != _settings.WebhookSecret)
            {
                _logger.LogWarning("Webhook received with invalid or missing verif-hash.");
                return Unauthorized();
            }
        }

        WebhookPayload? payload;
        try
        {
            payload = await Request.ReadFromJsonAsync<WebhookPayload>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialise webhook payload.");
            return BadRequest();
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.TxRef))
            return BadRequest();

        // Only act on charge events — ignore refunds and other event types.
        if (!string.IsNullOrWhiteSpace(payload.Event) &&
            !payload.Event.StartsWith("charge.", StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        await _payments.HandleWebhookAsync(payload.TxRef, payload.Status, ct);
        return Ok();
    }
}
