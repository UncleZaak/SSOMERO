FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the project files and restore inside the container. Restoring after copying
# the full source ensures any NuGet/Directory.Build.* files and additional props
# are present during restore. Also set a deterministic package cache location.
COPY . ./

# Use a dedicated nuget packages folder inside the container to avoid permission
# issues with the default cache and to keep the build reproducible.
ENV NUGET_PACKAGES=/root/.nuget/packages
RUN mkdir -p /root/.nuget/packages \
	&& dotnet restore Ssomero.Api.csproj

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

# Allow Cloud Run to choose the port via the PORT env var; default to 8080 when not set.
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck uses localhost and the container's port; keep it simple for local testing
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://localhost:8080/api/health || exit 1

# Use a shell wrapper so the container respects the PORT environment variable at runtime
ENTRYPOINT ["sh", "-lc", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet Ssomero.Api.dll"]
