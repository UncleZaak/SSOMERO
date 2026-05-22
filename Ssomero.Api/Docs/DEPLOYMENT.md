# Ssomero Deployment and Production Hardening Guide

This document outlines deployment, production hardening, monitoring, and rollback procedures for the Ssomero platform.

## 1. Environment configuration

- All secrets MUST be provided via environment variables or a secure secret store (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault).
- Do not commit secrets to source control.

Recommended environment variables (examples):

- JWT__Secret
- ConnectionStrings__Default
- ConnectionStrings__Hangfire
- AzureStorage__ConnectionString
- Payment__SecretKey
- EmailSettings__Password
- ADMIN_EMAIL
- ADMIN_PASSWORD

## 2. Building and running

Local dev:

- dotnet restore
- dotnet build
- dotnet run --project Ssomero.Api

Release build:

- dotnet build -c Release
- dotnet publish -c Release -o ./publish

## 3. HTTPS and Reverse Proxies

- Ensure reverse proxy forwards X-Forwarded-For and X-Forwarded-Proto headers.
- The API configures forwarded headers and HSTS for non-development.

## 4. Database

- Use managed DB when possible (Azure SQL / AWS RDS / Google Cloud SQL / PostgreSQL managed service).
- Backup policy: daily snapshot, retain 30 days, weekly full backups kept for 90 days.
- Test restores monthly.
- Apply migrations during maintenance windows. Prefer manual migrations in production.

## 5. Logging and Monitoring

- Serilog configured with console and rolling file sinks. In cloud, route logs to platform logging or a log shipper.
- Integrate Application Insights, Sentry, or Prometheus + Grafana for metrics.
- Expose Prometheus metrics endpoint and use alerting rules for error rate and latency.

## 6. CI/CD

- Use pipeline to build, test, and publish artifacts.
- Deploy to staging first. Run smoke tests. Promote artifacts to production using the same build.
- Rollback: redeploy previous artifact. Maintain DB migration reversibility policy.

## 7. MAUI

- Use SecureStorage for tokens.
- Use Release configurations and sign Android/iOS builds.

## 8. Health checks

- /api/health — liveness
- /api/health/ready — readiness (DB & cache)

## 9. Rollback

- Keep at least two previous deployments available.
- On critical failure, redeploy previous build and restore DB from pre-deploy backup if needed.

## 10. Secret rotation

- Rotate JWT secret and API keys periodically. Revoke and generate new keys as required.

## 11. Contact

- Platform owner: ADMIN_EMAIL (set via environment variable)

---

This guide is a starting point; adjust to your hosting provider and compliance requirements.
