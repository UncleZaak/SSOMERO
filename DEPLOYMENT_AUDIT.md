# Deployment Audit for Ssomero.Api

This document summarizes the deployment readiness audit and PASS/FAIL status for key areas required to deploy Ssomero.Api to Google Cloud Run.

- Build readiness: PASS
  - dotnet build and dotnet publish succeed locally. publish output produced.

- Docker readiness: PASS (after Dockerfile adjustments)
  - Dockerfile updated to set NUGET_PACKAGES and respect PORT.
  - Image builds successfully with `docker build -f Ssomero.Api/Dockerfile -t ssomero-api:local Ssomero.Api`.

- Cloud Run readiness: PARTIAL
  - Container can start, listens on PORT env var, and responds to /api/health when required secrets are provided.
  - Production requires additional configuration: secrets, Cloud SQL, optional Redis/Hangfire persistence.

- Cloud SQL readiness: PARTIAL
  - App defaults to SQLite in development.
  - Program.cs supports switching provider via `Database:Provider` config key.
  - Npgsql provider package added to project to enable PostgreSQL support in production.

- JWT readiness: PARTIAL
  - Jwt settings read from configuration section `Jwt`.
  - `Jwt:Secret` must be provided via `JWT__Secret` environment variable in Production.

- Health endpoint readiness: PASS
  - `/api/health` (liveness) and `/api/health/ready` (readiness) implemented.

- Secrets readiness: FAIL (requires provisioning)
  - Required production secrets are not present in appsettings. They must be provided via environment or Secret Manager.

- Logging readiness: PASS
  - Serilog configured to write to console and file; stdout captured by container runtime.

- Background jobs (Hangfire) readiness: PARTIAL
  - Hangfire configured with InMemory fallback for development. For production persistent storage is recommended; ConnectionStrings:Hangfire must be set and provider configured.

Notes and action items
- Provide required production secrets using Google Secret Manager and inject via Cloud Run environment variables.
- Provision Cloud SQL (Postgres) and set Database:Provider to `postgresql` in production config.
- Configure Redis and Hangfire persistent storage if multi-instance/persistent background jobs are required.
