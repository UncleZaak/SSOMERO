Production Deployment Guide

Environment variables (required):
- JWT__Secret - signing secret for JWT (minimum 32 chars)
- ConnectionStrings__Default - production database connection string
- ConnectionStrings__Hangfire - (optional) Hangfire database
- AzureStorage__ConnectionString - (optional) Azure Blob Storage connection string
- Metrics__ApiKey - (optional) key for /metrics endpoint
- Admin__Email, Admin__Password - initial admin account credentials

Do NOT commit these values to source control. Use your cloud provider's secret store or GitHub Actions secrets for CI/CD.
