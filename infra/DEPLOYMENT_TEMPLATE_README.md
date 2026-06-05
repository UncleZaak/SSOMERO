Deployment Templates and Environment Guidance

Files:
- docker-compose.prod.yml - production compose with Redis and environment variables
- .env.example - environment variable list

Staging vs Production:
- Use separate environment variables and separate containers/tags for staging (e.g., ssomero/api:staging).
- Use lower resource limits in staging and enable verbose logging only in staging.

Secrets:
- Use a secret store (Azure KeyVault, AWS Secrets Manager) or CI secrets to inject values during deployment. Never commit secrets.

Rollback:
- Maintain previous image tags in registry. To rollback, update docker-compose to use previous tag and redeploy.
