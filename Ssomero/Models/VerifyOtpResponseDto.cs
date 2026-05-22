namespace Ssomero.Models;

public class VerifyOtpResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
}
