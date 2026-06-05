Security Audit Summary

Findings:
- No production secrets committed in appsettings. Production files contain empty placeholders.
- HSTS and HTTPS redirection applied in non-development.
- Security headers added via middleware.
- Rate limiting configured for auth-related endpoints.

Risks:
- Ensure JWT__Secret is long and random; rotate keys periodically.
- Ensure storage/account keys are stored in secure secret store and not in environment variables in shared CI logs.

Recommendations:
- Integrate secret scanning in CI.
- Add automated smoke tests post-deploy to verify /api/health and SignalR connectivity.
