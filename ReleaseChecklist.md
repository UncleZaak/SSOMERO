Release Checklist

- Ensure no debug-only services are registered in Release.
- Confirm appsettings.Production.json contains no secrets; set required env vars in the deployment environment.
- Verify MAUI Release build produces signed AAB and passes smoke test on a physical device.
- Run API migration on startup with maintenance window.
- Verify health endpoints respond: /api/health and /api/health/ready.
- Verify metrics endpoint /metrics is protected in Production.
- Confirm logging configuration points to production sink (Seq/Application Insights) via env vars.
- Confirm rate limiting and request size limits are adequate.
