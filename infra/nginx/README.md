Nginx Reverse Proxy Example and Notes

Why: TLS termination, WebSocket forwarding for SignalR, and security headers should be handled at the reverse proxy.

Key recommendations:
- Terminate TLS at the proxy and forward X-Forwarded-* headers.
- Set proxy_http_version 1.1 and Connection/Upgrade headers to allow WebSocket upgrades.
- Set client_max_body_size to match API request size limits.
- Add security headers and HSTS (example config included).
- For Cloudflare users: enable "WebSockets" in Cloudflare dashboard and configure origin rules.

Compatibility:
- Works for Docker, VPS, Azure with custom Nginx-based fronting, and Cloudflare.

Rollback:
- Keep original API service reachable on internal port and revert proxy to simple TCP passthrough to rollback.
