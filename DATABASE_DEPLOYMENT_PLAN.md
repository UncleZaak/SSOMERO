# Database Deployment Plan for Ssomero.Api

Objective: Preserve local SQLite development experience while enabling PostgreSQL (Cloud SQL) in production.

Current state
- Program.cs defaults to SQLite and supports switching via `Database:Provider`.
- Ssomero.Api.csproj now includes Microsoft.EntityFrameworkCore.Sqlite and Npgsql.EntityFrameworkCore.PostgreSQL.

Plan
1. Development (local)
   - Keep Database:Provider = sqlite in appsettings.json or development config.
   - Continue to use SQLite (Data Source=ssomero.db) for developer machines.

2. Production (Cloud Run)
   - Provision Cloud SQL (Postgres). Create an instance and database.
   - Store the connection string in Secret Manager as `projects/PROJECT_ID/secrets/SSOMERO_DB_CONN:latest`.
   - Set `Database:Provider` to `postgresql` via Cloud Run env var or appsettings.Production.json override.
   - Set `ConnectionStrings__Default` env var to the Cloud SQL connection string.

3. EF Migrations
   - Continue to manage EF migrations locally and commit them to repo (`dotnet ef migrations add` as required).
   - On first deployment, the app runs `db.Database.MigrateAsync()` at startup (Program.cs) to apply migrations.

Notes
- Ensure the Cloud Run service connects to Cloud SQL using the Cloud SQL Auth Proxy or the built-in Cloud Run connector (use `--add-cloudsql-instances` and set appropriate connection string).
- No schema changes required in code. Switch of provider is controlled by configuration.
