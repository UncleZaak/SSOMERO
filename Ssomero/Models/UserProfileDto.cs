namespace Ssomero.Models;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string SelfieUrl { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Institution { get; set; } = string.Empty;
    public bool IsSubscriptionActive { get; set; }
    public bool IsSubscriptionExempt { get; set; }
}
