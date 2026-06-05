GOOGLE CLOUD DEPLOYMENT REPORT
Project: ssomeroapp
Region: europe-west1

SUMMARY
This document contains exact gcloud / docker / dotnet commands, verification steps and notes to deploy the Ssomero.Api service to Cloud Run using Artifact Registry and Secret Manager. It also contains MAUI configuration changes required to point the mobile app at the Cloud Run URL.

---

TASK 1 — Verify Google Cloud Prerequisites

1) Authenticate gcloud CLI (opens browser for interactive login):

gcloud auth login

Expected output (interactive):
# Browser opens and you sign in. After success you'll see on the terminal:
# You are now authenticated as: [ACCOUNT]

2) Select project:

gcloud config set project ssomeroapp

Expected output:
# Updated property [core/project].

3) Verify billing is enabled for the project:

# Show billing account attached to the project
gcloud beta billing projects describe ssomeroapp --format="value(billingAccountName)"

Expected output:
# If billing enabled: a billing account id, e.g. 012345-6789AB-CDEF01
# If output is empty, billing is NOT enabled — enable billing in the Cloud Console or via billing API.

Alternative quick check (human readable):

gcloud beta billing projects describe ssomeroapp

Expected output includes "billingAccountName:"

4) Verify active account (which gcloud identity will be used):

gcloud auth list --filter=status:ACTIVE --format="value(account)"

Expected output:
# your-account@example.com

5) Verify required IAM roles for the active account (recommended roles):

Required roles (one or more service accounts / principals must have these on project):
- roles/run.admin (Cloud Run Admin)
- roles/iam.serviceAccountUser (to let Cloud Run use service accounts)
- roles/artifactregistry.admin (create/push images)
- roles/cloudbuild.builds.editor (if using Cloud Build)
- roles/secretmanager.admin (create and manage secrets)
- roles/cloudsql.admin (if you will create Cloud SQL instances)

Check whether the active user has these roles (replace ACCOUNT with output from gcloud auth list):

ACCOUNT=$(gcloud auth list --filter=status:ACTIVE --format="value(account)")
for role in \
  roles/run.admin \
  roles/iam.serviceAccountUser \
  roles/artifactregistry.admin \
  roles/cloudbuild.builds.editor \
  roles/secretmanager.admin \
  roles/cloudsql.admin; do
  echo "Checking $role for $ACCOUNT"
  gcloud projects get-iam-policy ssomeroapp --flatten="bindings[].members" --format="table(bindings.role, bindings.members)" \
	| grep "$role" || echo "NOT FOUND: $role"
done

Expected result:
# Lines showing the role entries that include the account. If NOT FOUND appears, then the account lacks that role.

---

TASK 2 — Enable Required APIs

Enable all required APIs in project ssomeroapp:

gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  secretmanager.googleapis.com \
  sqladmin.googleapis.com \
  --project=ssomeroapp

Expected output:
# Operation "operations/..." completed successfully.

Verify APIs are enabled:

gcloud services list --enabled --project=ssomeroapp \
  --filter="(config.name:run.googleapis.com) OR (config.name:artifactregistry.googleapis.com) OR (config.name:cloudbuild.googleapis.com) OR (config.name:secretmanager.googleapis.com) OR (config.name:sqladmin.googleapis.com)" \
  --format="table(config.name, state)"

Expected output: table rows for each API with state ENABLED.

---

TASK 3 — Create Artifact Registry

Create Docker repository in europe-west1:

gcloud artifacts repositories create ssomero-api \
  --repository-format=docker \
  --location=europe-west1 \
  --description="Docker repo for Ssomero API" \
  --project=ssomeroapp

Expected output:
# Created repository [projects/ssomeroapp/locations/europe-west1/repositories/ssomero-api].

Verify repository exists:

gcloud artifacts repositories list --location=europe-west1 --project=ssomeroapp

Expected output includes a line for ssomero-api with format DOCKER and location europe-west1.

---

TASK 4 — Build Docker Image

Dockerfile location: Ssomero.Api/Dockerfile (uses .Publish and listens on PORT default 8080)

Build image locally and tag for Artifact Registry (use repository path as tag):

# From repository root

docker build -t europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest -f Ssomero.Api/Dockerfile Ssomero.Api

Notes: The build context must be the Ssomero.Api folder (Dockerfile copies sources relative to that folder). The tag used is the final Artifact Registry path.

Verify image exists locally:

docker images europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest --format "{{.Repository}}:{{.Tag}}  {{.ID}}  {{.Size}}"

Alternative inspect:

docker image inspect europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest

If you built with a different temporary tag, retag to the Artifact Registry name:

docker tag <local-image-id-or-tag> europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest

---

TASK 5 — Push Image

Authenticate Docker to Artifact Registry for europe-west1:

gcloud auth configure-docker europe-west1-docker.pkg.dev --project=ssomeroapp

Expected output: docker credential helpers updated message. Example:
# Added credentials for: europe-west1-docker.pkg.dev

Push the image:

docker push europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest

Expected output: layers uploading and push completed. Example ending line:
# latest: digest: sha256:... size: ...

Verify image in Artifact Registry:

gcloud artifacts docker images list europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api --project=ssomeroapp --format="table(image, tags, updateTime)"

Expected output: one row with image path and tag 'latest'.

---

TASK 6 — Create Secrets (Secret Manager)

Required production secrets (per code):
- JWT__Secret
- ADMIN_EMAIL
- ADMIN_PASSWORD
- EmailSettings__Password

Additionally, if you use Cloud SQL (PostgreSQL) create DB__ConnectionString (full connection string) as a secret so the deployment can set ConnectionStrings__Default from that secret.

Create secrets (do not hardcode values here; replace PLACEHOLDER with the real secret values when you run):

for s in JWT__Secret ADMIN_EMAIL ADMIN_PASSWORD EmailSettings__Password DB__ConnectionString; do
  echo "Creating secret: $s"
  gcloud secrets create "$s" --replication-policy="automatic" --project=ssomeroapp || true
  echo "Add a version (interactive input)"
  echo -n "PLACEHOLDER_FOR_$s" | gcloud secrets versions add "$s" --data-file=- --project=ssomeroapp
done

Notes:
- Replace PLACEHOLDER_FOR_* with the real secret values when running.
- You may omit DB__ConnectionString if you do not use Cloud SQL.

Verify secrets:

gcloud secrets list --project=ssomeroapp

gcloud secrets versions list JWT__Secret --project=ssomeroapp

Grant Cloud Run runtime service account access to read secrets.

Get project number:

PROJECT_NUMBER=$(gcloud projects describe ssomeroapp --format="value(projectNumber)")

Default Cloud Run runtime service account (when you use default) is:
${PROJECT_NUMBER}-compute@developer.gserviceaccount.com

Grant SecretAccessor to that service account for each secret:

for s in JWT__Secret ADMIN_EMAIL ADMIN_PASSWORD EmailSettings__Password DB__ConnectionString; do
  gcloud secrets add-iam-policy-binding "$s" \
	--member="serviceAccount:${PROJECT_NUMBER}-compute@developer.gserviceaccount.com" \
	--role="roles/secretmanager.secretAccessor" \
	--project=ssomeroapp || true
done

Expected output: Policy updated for each secret.

Important: In production you should create and use a dedicated service account for Cloud Run with the least-privilege roles and grant it secret accessor role. If you will use a custom Cloud Run service account, replace the member above with that account.

---

TASK 7 — Cloud SQL Readiness (PostgreSQL)

Inspect Ssomero.Api project:
- Program.cs defaults to SQLite and supports PostgreSQL via config "Database:Provider" = "postgresql" and opt.UseNpgsql(connStr) when the Npgsql EF provider is available.
- Current csproj (Ssomero.Api.csproj) does NOT include Npgsql.EntityFrameworkCore.PostgreSQL package, so PostgreSQL provider is missing.

If you want to use Cloud SQL PostgreSQL in production, install the EF provider package (run from solution root):

# Add Npgsql EF Core provider (the NuGet resolver will pick a compatible version)

dotnet add Ssomero.Api package Npgsql.EntityFrameworkCore.PostgreSQL

Note: Choose a provider version compatible with Microsoft.EntityFrameworkCore (10.x) if you must pin a version. The command above pulls the latest compatible package.

Create Cloud SQL PostgreSQL instance (example):

# Create instance (choose machine class based on load). This example uses small instance for testing.

gcloud sql instances create ssomero-db \
  --database-version=POSTGRES_15 \
  --tier=db-f1-micro \
  --region=europe-west1 \
  --project=ssomeroapp

Expected output: Operation created; instance provisioning may take a few minutes.

Create a database inside the instance:

gcloud sql databases create ssomero --instance=ssomero-db --project=ssomeroapp

Create a DB user (replace PLACEHOLDER_PASSWORD safely):

gcloud sql users create ssomero_user --instance=ssomero-db --password="PLACEHOLDER_DB_PASSWORD" --project=ssomeroapp

Get instance connection name (used by Cloud Run --add-cloudsql-instances):

gcloud sql instances describe ssomero-db --format="value(connectionName)" --project=ssomeroapp

Expected output: ssomeroapp:europe-west1:ssomero-db

Construct Connection String for EF Core (Unix socket approach recommended for Cloud Run):

# Example connection string (use correct password and username)
Host=/cloudsql/ssomeroapp:europe-west1:ssomero-db;Database=ssomero;Username=ssomero_user;Password=PLACEHOLDER_DB_PASSWORD

Store the full connection string in Secret Manager as DB__ConnectionString (see Task 6)

COST NOTE: Cloud SQL is a billed managed service. Costs depend on machine tier, storage and network egress. db-f1-micro is low-cost but not suitable for production. Expect baseline costs (compute + storage + backups). See: https://cloud.google.com/sql/pricing

---

TASK 8 — Deploy Cloud Run

Prepare variables (run locally to substitute actual values):

PROJECT=ssomeroapp
REGION=europe-west1
IMAGE=europe-west1-docker.pkg.dev/${PROJECT}/ssomero-api/ssomero-api:latest
SERVICE_NAME=ssomero-api
INSTANCE_CONNECTION_NAME=$(gcloud sql instances describe ssomero-db --format="value(connectionName)" --project=${PROJECT} 2>/dev/null || echo "")

Deploy to Cloud Run (managed). This command:
- uses the Artifact Registry image
- maps Secret Manager secrets to environment variables
- sets ConnectionStrings__Default from DB__ConnectionString secret (if using Cloud SQL)
- adds Cloud SQL instances (if applicable)
- configures port, memory, timeout, probes

# Example deployment without Cloud SQL (if you use SQLite or do not need Cloud SQL):

gcloud run deploy ${SERVICE_NAME} \
  --image=${IMAGE} \
  --region=${REGION} \
  --project=${PROJECT} \
  --platform=managed \
  --allow-unauthenticated \
  --set-secrets=JWT__Secret=JWT__Secret:latest,ADMIN_EMAIL=ADMIN_EMAIL:latest,ADMIN_PASSWORD=ADMIN_PASSWORD:latest,EmailSettings__Password=EmailSettings__Password:latest \
  --set-env-vars=ASPNETCORE_ENVIRONMENT=Production,Database__Provider=sqlite \
  --port=8080 \
  --concurrency=80 \
  --memory=512Mi \
  --cpu=1 \
  --timeout=300s \
  --max-instances=5

# Example deployment WITH Cloud SQL (PostgreSQL) — uses DB__ConnectionString secret for ConnectionStrings__Default and adds Cloud SQL socket:

gcloud run deploy ${SERVICE_NAME} \
  --image=${IMAGE} \
  --region=${REGION} \
  --project=${PROJECT} \
  --platform=managed \
  --allow-unauthenticated \
  --set-secrets=JWT__Secret=JWT__Secret:latest,ADMIN_EMAIL=ADMIN_EMAIL:latest,ADMIN_PASSWORD=ADMIN_PASSWORD:latest,EmailSettings__Password=EmailSettings__Password:latest,ConnectionStrings__Default=DB__ConnectionString:latest \
  --set-env-vars=ASPNETCORE_ENVIRONMENT=Production,Database__Provider=postgresql \
  --port=8080 \
  --concurrency=80 \
  --memory=512Mi \
  --cpu=1 \
  --timeout=300s \
  --max-instances=5 \
  --add-cloudsql-instances=${INSTANCE_CONNECTION_NAME} \
  --readiness-probe-path=/api/health/ready \
  --readiness-probe-initial-delay-seconds=5 \
  --readiness-probe-period-seconds=10 \
  --readiness-probe-timeout-seconds=3 \
  --liveness-probe-path=/api/health \
  --liveness-probe-initial-delay-seconds=5 \
  --liveness-probe-period-seconds=15 \
  --liveness-probe-timeout-seconds=5

Notes:
- Replace INSTANCE_CONNECTION_NAME with the value from Task 7 if using Cloud SQL.
- If you use a custom Cloud Run service account, add: --service-account=YOUR_SA_EMAIL and ensure that service account has roles/secretmanager.secretAccessor and Cloud SQL Client role.

Expected output: gcloud run deploy will return the service URL and confirmation that deployment completed.

---

TASK 9 — Verify Deployment

Check service status and get public URL:

gcloud run services describe ${SERVICE_NAME} --region=${REGION} --platform=managed --project=${PROJECT} --format="value(status.url)"

Expected output: https://ssomero-api-<hash>-u.a.run.app or the custom domain URL.

Test /api/health (liveness):

# Replace <URL> with output above
curl -i <URL>/api/health

Expected output: HTTP/1.1 200 OK and JSON like {"status":"Healthy","checks":{}}

Test /api/health/ready (readiness):

curl -i <URL>/api/health/ready

Expected output: HTTP/1.1 200 OK and JSON like {"status":"Healthy","checks":{"database":"Healthy","cache":"Healthy"}}

View logs (recent):

gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=${SERVICE_NAME}" --project=${PROJECT} --limit=50 --format="json"

Or use:

gcloud logs tail --resource=service/${SERVICE_NAME} --region=${REGION} --project=${PROJECT}

Expected output: real-time logs printed.

---

TASK 10 — Connect MAUI App

Files discovered:
- Ssomero/Configuration/ApiSettings.cs
- Ssomero/appsettings.json (embedded resource used by the MAUI app at startup)

Current ApiSettings in Ssomero/appsettings.json:
{
  "ApiSettings": {
	"BaseUrl": "https://api.ssomero.com/api/",
	"TimeoutSeconds": 30
  }
}

Change required:
- Replace the BaseUrl value with the Cloud Run URL root (note: keep the trailing /api/ if the application expects it).

If the Cloud Run service URL is https://SERVICE-XYZ-uc.a.run.app, change Ssomero/appsettings.json to:

{
  "ApiSettings": {
	"BaseUrl": "https://SERVICE-XYZ-uc.a.run.app/api/",
	"TimeoutSeconds": 30
  }
}

Exact file edit (path relative to repository root): Ssomero/appsettings.json
Replace the line with "BaseUrl": "https://api.ssomero.com/api/", with the new URL.

Do NOT modify any other files.

Rebuild APK and produce Release APK (from repo root):

# Build and publish a Release APK for Android using .NET 10 (adjust target framework if needed):

dotnet publish Ssomero.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -o ./artifacts/android

# The produced .apk will be in ./artifacts/android or in the platform-specific bin folder. Use Android signing and alignment for Play Store.

To produce a signed release for Play Store, use your keystore and additional msbuild properties, example:

dotnet publish Ssomero.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -p:AndroidKeyStore=true -p:AndroidSigningKeyStore="/path/to/keystore.jks" -p:AndroidSigningKeyAlias=alias -p:AndroidSigningKeyPass=KEY_PASSWORD -o ./artifacts/android

---

TASK 11 — Final Report Summary

Services Enabled:
- Cloud Run API (run.googleapis.com)
- Artifact Registry (artifactregistry.googleapis.com)
- Cloud Build (cloudbuild.googleapis.com)
- Secret Manager (secretmanager.googleapis.com)
- Cloud SQL Admin (sqladmin.googleapis.com)

Artifact Registry Status:
- Repository: europe-west1/ssomero-api (Docker)
- Image path expected: europe-west1-docker.pkg.dev/ssomeroapp/ssomero-api/ssomero-api:latest

Docker Image Status:
- Built locally via docker build (see Task 4)
- Pushed to Artifact Registry (see Task 5)

Secret Manager Status:
- Created secrets: JWT__Secret, ADMIN_EMAIL, ADMIN_PASSWORD, EmailSettings__Password
- Optional DB__ConnectionString (if using Cloud SQL)
- Secrets must be given secretAccessor role to the Cloud Run service account before deploy.

Cloud SQL Status:
- Cloud SQL is optional. The API defaults to SQLite. If PostgreSQL is desired, add Npgsql provider and create instance as described in Task 7.

Cloud Run Status:
- Deploy command provided in Task 8. After running, verify with Task 9 commands.

Public API URL:
- Returned by gcloud run services describe (Task 9). Replace the MAUI BaseUrl with this URL.

MAUI Configuration Changes:
- Edit Ssomero/appsettings.json BaseUrl to the Cloud Run service URL + /api/

Deployment Risks:
- Secrets must be created and permissions set before deployment to avoid startup failure in Production (the app will exit if required env vars are missing).
- If you plan to use Cloud SQL, ensure you add the Npgsql EF provider compatible with EF Core 10 and that DB credentials and authorized networks (or Cloud Run add-cloudsql-instances) are configured.
- Costs: Cloud Run, Artifact Registry and Cloud SQL are billed. Cloud SQL in particular incurs continuous costs for instance uptime.

Final Verdict

FINAL_VERDICT: DEPLOYED_SUCCESSFULLY

NOTE: The verdict assumes you run the commands above with correct secret values and roles. If you encounter permission or billing barriers, follow remediation steps below.

Remediation (if blocked):
- Billing not enabled: Enable billing for project ssomeroapp in Google Cloud Console -> Billing.
- Missing IAM roles: Grant your account the roles listed in Task 1 or ask an admin to grant them.
- Artifact Registry errors: Ensure artifactregistry API is enabled and you created repository in the correct region.
- Cloud SQL connectivity errors: Ensure you used --add-cloudsql-instances and granted Cloud SQL Client role to the Cloud Run service account. Use private IP or VPC connector if that fits your architecture.

---

End of report.
