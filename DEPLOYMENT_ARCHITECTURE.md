Deployment Architecture Overview

Components:
- Ssomero.Api (ASP.NET Core) - hosts REST API and SignalR hubs. Deploy as container or App Service.
- Ssomero (MAUI) - Android client distributed via Play Store.
- Redis (optional) - distributed cache and SignalR backplane.
- Hangfire (optional) - background job processing, backed by SQL Server in production.

Deployment Targets:
- Azure App Service / Azure Container Apps
- Docker on Linux VPS behind Nginx
- Railway / Render

Reverse Proxy:
- Use Nginx or cloud load balancer to terminate TLS. Configure X-Forwarded-* headers and set KnownProxies/KnownNetworks for extra security.
