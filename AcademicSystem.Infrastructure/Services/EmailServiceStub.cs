using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AcademicSystem.Application.Common.Interfaces;

namespace AcademicSystem.Infrastructure.Services
{
    public class EmailServiceStub : IEmailService
    {
        private readonly ILogger<EmailServiceStub> _logger;

        public EmailServiceStub(ILogger<EmailServiceStub> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            // Simple development stub — log the message. Production implementation should send via SMTP/SendGrid.
            _logger.LogInformation("[EmailStub] To: {To}; Subject: {Subject}; Body: {Body}", to, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
