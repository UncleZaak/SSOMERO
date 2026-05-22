namespace Ssomero.Api.Configuration;

/// <summary>
/// SMTP settings bound from "EmailSettings" configuration section.
/// Sensitive values (SenderEmail, Password) should be provided via
/// environment variables or dotnet user-secrets, NOT appsettings.json.
///
/// Environment variable format:
///   EmailSettings__SenderEmail=your@gmail.com
///   EmailSettings__Password=abcd efgh ijkl mnop
///
/// Or user-secrets:
///   dotnet user-secrets set "EmailSettings:SenderEmail" "your@gmail.com"
///   dotnet user-secrets set "EmailSettings:Password" "abcd efgh ijkl mnop"
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Ssomero App";
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutMs { get; set; } = 15000;
}
