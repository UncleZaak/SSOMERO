Monitoring & Observability Strategy

1. Structured logging: Serilog configured; add sink for Application Insights or Seq via environment variables.
2. Metrics: prometheus-net exposes /metrics (protected via API key in non-development).
3. Tracing: add OpenTelemetry integration to capture request traces and SignalR events.
4. Health checks: /api/health (liveness) and /api/health/ready (readiness) included for orchestration probes.
