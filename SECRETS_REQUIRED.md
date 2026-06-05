# Required Production Secrets for Ssomero.Api

This document lists required production secrets, their purpose, whether they are required, and recommended storage locations.

1. JWT__Secret
   - Purpose: Symmetric key used to sign and validate JWT access tokens.
   - Required?: Yes (app fails to start in Production without it)
   - Recommended storage: Google Secret Manager (accessed by Cloud Run runtime via environment variable or Secret Manager integration)

2. ADMIN_EMAIL
   - Purpose: Initial administrator email used by the application; validated at startup when in Production mode.
   - Required?: Yes
   - Recommended storage: Google Secret Manager or Cloud Run environment variable (not checked into repo)

3. ADMIN_PASSWORD
   - Purpose: Initial admin password for seeding and startup validation.
   - Required?: Yes
   - Recommended storage: Google Secret Manager

4. EmailSettings__Password
   - Purpose: SMTP password for sending emails; used by EmailSettings configuration.
   - Required?: Yes (Production expects it)
   - Recommended storage: Google Secret Manager

5. ConnectionStrings__Default
   - Purpose: Database connection string for production (e.g., Postgres on Cloud SQL)
   - Required?: Yes for production persistence (if using Cloud SQL)
   - Recommended storage: Google Secret Manager or Cloud Run env var referencing Secret Manager

6. ConnectionStrings__Redis (optional)
   - Purpose: Redis connection string for distributed cache (StackExchange.Redis)
   - Required?: No (app falls back to in-memory cache), but recommended for multi-instance
   - Recommended storage: Google Secret Manager

7. ConnectionStrings__Hangfire (optional)
   - Purpose: Connection string for Hangfire persistent storage
   - Required?: No (in-memory fallback used), but recommended for production durability
   - Recommended storage: Google Secret Manager

Recommended practices for Google Cloud
- Use Google Secret Manager to store secrets. Grant the Cloud Run service account permission to access secrets and inject them as environment variables during deployment.
- Never commit secrets to source control or appsettings files.
