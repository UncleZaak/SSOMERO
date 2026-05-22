using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Filters;

/// <summary>
/// Action filter that blocks access to an endpoint unless the authenticated student
/// has an active (non-expired) subscription.
/// Apply via <c>[RequiresActiveSubscription]</c> on any controller or action that
/// should be gated behind payment.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresActiveSubscriptionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var studentId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var paymentService = context.HttpContext.RequestServices
            .GetRequiredService<IPaymentService>();

        var sub = await paymentService.GetActiveSubscriptionAsync(studentId);
        if (sub is null || !sub.IsActive || sub.EndDate <= DateTime.UtcNow)
        {
            context.Result = new ObjectResult(new
            {
                error = "An active subscription is required to access this feature.",
                code  = "SUBSCRIPTION_REQUIRED"
            })
            { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
