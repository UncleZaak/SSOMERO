namespace Ssomero.Api.Entities;

/// <summary>
/// Represents a student's active subscription period.
/// Created exclusively by server-side payment verification — never by the client.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Student? Student { get; set; }

    public PaymentPlan Plan { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>The payment that created this subscription.</summary>
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }
}
