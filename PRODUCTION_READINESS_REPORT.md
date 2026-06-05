Ssomero Production Readiness Report

Summary:
- Backend (Ssomero.Api) is prepared for production with HSTS, response compression, rate-limiting, forwarded headers, request size limits, and health/readiness endpoints.
- Secrets removed from source: JWT secret and other production secrets must be injected via environment variables.
- Dockerfile and docker-compose provided for containerized deployments.
- CI workflows added for backend and MAUI Android builds.

Security Hardening:
- SecurityHeadersMiddleware adds CSP, X-Frame-Options, X-Content-Type-Options, and Permissions-Policy headers.
- HSTS enabled for non-development environments.
- HTTPS redirection in non-development.
- Rate limiting applied to sensitive endpoints.

Observability:
- Serilog configured via appsettings and file sink; /metrics endpoint is available and protected by API key in production.

Next Steps:
- Configure production secrets in your deployment environment (JWT__Secret, ConnectionStrings__Default, AzureStorage__ConnectionString, Admin credentials).
- Configure persistent Hangfire storage (SQL Server) and Redis for SignalR scale-out if needed.
- Integrate Application Insights/Seq via environment-configured sinks for production telemetry.
