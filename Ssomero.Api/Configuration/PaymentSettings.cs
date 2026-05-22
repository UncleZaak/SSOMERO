namespace Ssomero.Api.Configuration;

public sealed class PaymentSettings
{
    /// <summary>
    /// When true, payments are auto-approved server-side without calling Flutterwave.
    /// Use in development / test environments only.
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>Flutterwave secret key — required when UseMock is false.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Flutterwave webhook verification hash — must match the X-Flw-Signature header.
    /// Set this to the webhook secret configured in the Flutterwave dashboard.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Base URL for the Flutterwave v3 API.</summary>
    public string BaseUrl { get; set; } = "https://api.flutterwave.com/v3/";
}
