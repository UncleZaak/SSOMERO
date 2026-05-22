# ─── Build stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for layer-cached restore
COPY Ssomero.Api/Ssomero.Api.csproj Ssomero.Api/
RUN dotnet restore Ssomero.Api/Ssomero.Api.csproj

# Copy full source and publish
COPY . .
WORKDIR /src/Ssomero.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ─── Runtime stage ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system ssomero && adduser --system --ingroup ssomero ssomero

# Create writable directories for logs and uploads
RUN mkdir -p /app/logs /app/uploads && \
    chown -R ssomero:ssomero /app

COPY --from=build --chown=ssomero:ssomero /app/publish .

USER ssomero

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Liveness probe: /api/health — fast, no dependency checks
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget -qO- http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "Ssomero.Api.dll"]
