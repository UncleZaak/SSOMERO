# Cache and Background Jobs (Redis & Hangfire)

Redis
- Current behavior: Redis is optional. If ConnectionStrings:Redis is set and Redis packages registered, the app will use distributed cache. Otherwise it falls back to in-memory cache.
- Recommendation: For Cloud Run multi-instance deployments, provision a managed Redis (e.g., Memorystore on GCP) and provide ConnectionStrings__Redis secret.
- Action steps:
  1. Add StackExchange.Redis package to Ssomero.Api if you plan to use Redis.
  2. In Program.cs uncomment or add `builder.Services.AddStackExchangeRedisCache(...)` to configure Redis when connection string is provided.

Hangfire
- Current behavior: Hangfire uses SQL Server storage if ConnectionStrings:Hangfire is present; otherwise uses InMemory storage as fallback.
- Recommendation: Use persistent Hangfire storage for production (Cloud SQL/Postgres or SQL Server). For PostgreSQL, consider using Hangfire.PostgreSql storage provider.
- Action steps:
  1. Choose Hangfire storage provider compatible with your DB (e.g., Hangfire.PostgreSql) and add package.
  2. Set ConnectionStrings__Hangfire to the DB connection string and configure Hangfire to use that storage.

Risk and notes
- Running Hangfire with in-memory storage in Cloud Run can lose job data when instances stop. Use persistent storage.
- Redis is recommended for distributed cache and to reduce load on DB.
