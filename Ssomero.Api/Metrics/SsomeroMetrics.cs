using Prometheus;

namespace Ssomero.Api.Metrics;

/// <summary>
/// Central registry of application-level Prometheus metrics.
/// All counters are created once at startup (static) and incremented from controllers.
/// Labels follow the convention: role/type = user category, status = success|failure|blocked.
/// </summary>
public static class SsomeroMetrics
{
    /// <summary>
    /// ssomero_logins_total{role, status}
    /// role  : Admin | Student | Lecturer
    /// status: success | failure | blocked
    /// </summary>
    public static readonly Counter LoginsTotal = Prometheus.Metrics.CreateCounter(
        "ssomero_logins_total",
        "Total login attempts.",
        new CounterConfiguration { LabelNames = ["role", "status"] });

    /// <summary>
    /// ssomero_registrations_total{type, status}
    /// type  : Student | Lecturer
    /// status: success | failure
    /// </summary>
    public static readonly Counter RegistrationsTotal = Prometheus.Metrics.CreateCounter(
        "ssomero_registrations_total",
        "Total registration attempts.",
        new CounterConfiguration { LabelNames = ["type", "status"] });

    /// <summary>ssomero_otp_sent_total — incremented every time an OTP is dispatched.</summary>
    public static readonly Counter OtpSentTotal = Prometheus.Metrics.CreateCounter(
        "ssomero_otp_sent_total",
        "Total OTPs sent.");

    /// <summary>
    /// ssomero_otp_verified_total{status}
    /// status: success | failure
    /// </summary>
    public static readonly Counter OtpVerifiedTotal = Prometheus.Metrics.CreateCounter(
        "ssomero_otp_verified_total",
        "Total OTP verification attempts.",
        new CounterConfiguration { LabelNames = ["status"] });
}
