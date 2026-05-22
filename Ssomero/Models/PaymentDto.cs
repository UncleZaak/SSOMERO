namespace Ssomero.Models;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public PaymentPlan Plan { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UGX";
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FailureReason { get; set; }
    public string? ReceiptUrl { get; set; }
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public string Plan { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UGX";
    public string Status { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? ReceiptUrl { get; set; }

    // Derived
    public string StatusLabel => Status;
    public string AmountLabel => $"{Currency} {Amount:N0}";
    public string DateLabel => CreatedAt.ToLocalTime().ToString("MMM dd, yyyy HH:mm");
}

public enum PaymentPlan
{
    Monthly,
    Semester
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Expired,
    Refunded
}
