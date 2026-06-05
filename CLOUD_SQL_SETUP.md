# Cloud SQL Setup for Ssomero.Api (Postgres)

Exact gcloud commands (replace PROJECT_ID, REGION, INSTANCE_NAME, DB_USER, DB_PASS):

1. Create Cloud SQL Postgres instance

```
gcloud sql instances create INSTANCE_NAME \
  --database-version=POSTGRES_15 \
  --cpu=1 --memory=4GiB --region=REGION \
  --project=PROJECT_ID
```

2. Create database and user

```
gcloud sql databases create ssomero --instance=INSTANCE_NAME --project=PROJECT_ID

gcloud sql users create db_user --instance=INSTANCE_NAME --password=DB_PASS --project=PROJECT_ID
```

3. Get connection string (private IP or public with SSL). For Cloud Run use Cloud SQL connection via Unix socket or Cloud SQL Auth proxy integration when deploying.

Example connection string using Cloud SQL connection name (use in EF connection string):
`Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=ssomero;Username=db_user;Password=DB_PASS;`  

4. Grant Cloud Run service account permission to connect to Cloud SQL

```
gcloud projects add-iam-policy-binding PROJECT_ID \
  --member=serviceAccount:PROJECT_NUMBER-compute@developer.gserviceaccount.com \
  --role=roles/cloudsql.client
```

5. During Cloud Run deploy, attach Cloud SQL instance:

```
gcloud run deploy ssomero-api \
  --image gcr.io/PROJECT_ID/ssomero-api:TAG \
  --add-cloudsql-instances=PROJECT_ID:REGION:INSTANCE_NAME \
  --set-env-vars ConnectionStrings__Default='Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=ssomero;Username=db_user;Password=DB_PASS;' \
  --project=PROJECT_ID --region=REGION --platform=managed
```

Notes
- Alternatively use Cloud SQL Auth proxy during build or set `DB_HOST`, `DB_USER`, `DB_PASS` separately and construct the connection string in Cloud Run.
- Ensure EF migrations are applied at startup (Program.cs contains Database.MigrateAsync()).
