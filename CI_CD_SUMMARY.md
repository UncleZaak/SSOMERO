CI/CD Summary

GitHub Actions workflows added:
- .github/workflows/backend.yml - builds, tests, and publishes backend artifact.
- .github/workflows/maui-android.yml - builds MAUI Android Debug and Release artifacts on windows-latest.

Secrets & Signing:
- Store signing keys and deployment credentials in GitHub Actions secrets; workflows should reference them at runtime and never print them to logs.

Deployment:
- Backend: Docker image push and deployment to registry (add step to workflow with appropriate secrets and registry config).
- MAUI: Signing must be performed in workflow using secure keystore from secrets.
