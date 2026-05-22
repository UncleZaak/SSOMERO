using System;
using System.Threading.Tasks;

namespace AcademicSystem.Application.Common.Interfaces
{
    public interface IPaymentGateway
    {
        /// <summary>
        /// Initiate a payment and return a redirect URL or payment reference.
        /// </summary>
        Task<string> CreatePaymentAsync(decimal amount, string currency, Guid userId);

        /// <summary>
        /// Verify webhook signature and process incoming webhook payload.
        /// Implementations must be idempotent.
        /// </summary>
        Task ProcessWebhookAsync(string payload, string signature);
    }
}
