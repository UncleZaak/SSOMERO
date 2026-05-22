using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssomero.Api.Configuration;

namespace Ssomero.Api.Services;

public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning(
                "EmailSettings:SenderEmail or EmailSettings:Password is not configured. "
                + "Set them via environment variables (EmailSettings__SenderEmail / EmailSettings__Password) "
                + "or dotnet user-secrets before sending emails.");
        }
    }

    /// <summary>
    /// Sends an email with the specified subject and body.
    /// Throws on failure so callers can surface the error.
    /// </summary>
    public virtual async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.Password))
            throw new InvalidOperationException(
                "SMTP credentials are not configured. "
                + "Set EmailSettings:SenderEmail and EmailSettings:Password via environment variables or user-secrets.");

        using var message = new MailMessage();
        message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = false;

        using var client = new SmtpClient(_settings.SmtpServer, _settings.Port);
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password);
        client.EnableSsl = _settings.EnableSsl;
        client.Timeout = _settings.TimeoutMs;

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} with subject \"{Subject}\"", toEmail, subject);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Email}: {StatusCode}", toEmail, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    /// <summary>
    /// Sends an OTP email to the specified address.
    /// Throws on failure so the caller can handle it.
    /// </summary>
    public Task SendOtpEmailAsync(string recipientEmail, string otpCode)
    {
        return SendEmailAsync(
            recipientEmail,
            "Your OTP Code",
            $"Your OTP code is: {otpCode}. It expires in 10 minutes.");
    }
}
