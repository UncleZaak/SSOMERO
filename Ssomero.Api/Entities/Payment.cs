using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public enum PaymentStatus { Pending, Completed, Failed, Cancelled, Expired, Refunded }

public enum PaymentPlan { Monthly, Semester }

public enum PaymentProvider { Flutterwave, Mock }

/// <summary>
/// Records a payment attempt. Never trust the client-reported status —
/// the status here is set exclusively by server-side verification.
/// </summary>
public class Payment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Student? Student { get; set; }

    /// <summary>Plan the student is paying for.</summary>
    public PaymentPlan Plan { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "UGX";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public PaymentProvider Provider { get; set; } = PaymentProvider.Mock;

    /// <summary>Provider-issued transaction reference (e.g. Flutterwave tx_ref).</summary>
    [Required, MaxLength(200)]
    public string ExternalReference { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Human-readable failure reason set when Status = Failed or Cancelled.</summary>
    [MaxLength(500)]
    public string? FailureReason { get; set; }

    /// <summary>URL to a generated receipt (set after successful completion).</summary>
    [MaxLength(1000)]
    public string? ReceiptUrl { get; set; }
}
