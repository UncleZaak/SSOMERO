# Google Cloud Deployment for Ssomero.Api

This document contains commands to build, push, and deploy the Ssomero.Api container to Google Cloud Run and configure secrets and Cloud SQL.

Replace PROJECT_ID, REGION, IMAGE_TAG, INSTANCE_NAME, DB_USER, DB_PASS accordingly.

1. Build and push image to Artifact Registry (or Container Registry)

```
# Build
docker build -f Ssomero.Api/Dockerfile -t gcr.io/PROJECT_ID/ssomero-api:IMAGE_TAG Ssomero.Api

# Push
docker push gcr.io/PROJECT_ID/ssomero-api:IMAGE_TAG
```

(Or using Cloud Build)
```
gcloud builds submit --tag gcr.io/PROJECT_ID/ssomero-api:IMAGE_TAG
```

2. Store secrets in Secret Manager

```
echo -n "SOME_SECRET_VALUE" | gcloud secrets create JWT_SECRET --data-file=- --project=PROJECT_ID
# Repeat for ADMIN_EMAIL, ADMIN_PASSWORD, EmailSettings__Password, DB_CONN
```

3. Deploy to Cloud Run attaching Cloud SQL

```
gcloud run deploy ssomero-api \
  --image gcr.io/PROJECT_ID/ssomero-api:IMAGE_TAG \
  --region=REGION \
  --platform=managed \
  --add-cloudsql-instances=PROJECT_ID:REGION:INSTANCE_NAME \
  --set-env-vars JWT__Secret="projects/PROJECT_ID/secrets/JWT_SECRET:latest",ADMIN_EMAIL="projects/PROJECT_ID/secrets/ADMIN_EMAIL:latest",ADMIN_PASSWORD="projects/PROJECT_ID/secrets/ADMIN_PASSWORD:latest",EmailSettings__Password="projects/PROJECT_ID/secrets/EMAIL_PW:latest",ConnectionStrings__Default='Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=ssomero;Username=DB_USER;Password=DB_PASS;' \
  --allow-unauthenticated
```

4. Verification

```
# Check service
gcloud run services describe ssomero-api --region=REGION --project=PROJECT_ID

# Test health
curl https://ssomero-api-<hash>-uc.a.run.app/api/health
curl https://ssomero-api-<hash>-uc.a.run.app/api/health/ready
```

5. Rollback

```
# To rollback to previous revision
gcloud run revisions list --service ssomero-api --region=REGION
gcloud run services update-traffic ssomero-api --to-revisions previous_revision=100 --region=REGION
```
