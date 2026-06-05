# Cloud Run Readiness for Ssomero.Api

Checks and PASS/FAIL

- Container startup: PASS (after Dockerfile fixes)
  - Image builds and container can start. Verified with docker build and docker run.

- Health checks: PASS
  - /api/health and /api/health/ready implemented and return JSON.

- Port binding: PASS
  - Dockerfile ENTRYPOINT respects PORT env var; container binds to ASPNETCORE_URLS=http://+:${PORT:-8080}.

- Environment variable support: PASS
  - App reads configuration via IConfiguration and supports environment variables (e.g., JWT__Secret, ConnectionStrings__Default).

- Secret support: PASS (via environment variables/Secret Manager)
  - Provide required secrets via Cloud Run Secret Manager integration; app will read them as env vars.

Notes and recommendations
- Use Cloud Run service account with access to Secret Manager and Cloud SQL.
- Use `--set-secrets` or `--set-env-vars` to inject secrets during `gcloud run deploy`.
