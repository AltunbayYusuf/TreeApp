#!/bin/bash
set -euo pipefail

PROJECT_ID=$(curl -s "http://metadata.google.internal/computeMetadata/v1/project/project-id" -H "Metadata-Flavor: Google")
TOKEN=$(curl -s "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token" -H "Metadata-Flavor: Google" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

echo " Secrets ophalen..."
DB_PASSWORD=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/db-password/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

GEMINI_API_KEY=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/gemini-api-key/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

echo " Service account key ophalen voor Cloud SQL Proxy..."
curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/sa-key/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())" | sudo tee /tmp/sa-key.json > /dev/null
sudo chmod 644 /tmp/sa-key.json

ROOT_DOMAIN=$(curl -s -f "http://metadata.google.internal/computeMetadata/v1/instance/attributes/root-domain" -H "Metadata-Flavor: Google" || echo "echo20.com")
APP_SUBDOMAIN=$(curl -s -f "http://metadata.google.internal/computeMetadata/v1/instance/attributes/app-subdomain" -H "Metadata-Flavor: Google" || echo "treeapp")
GCS_BUCKET=$(curl -s -f "http://metadata.google.internal/computeMetadata/v1/instance/attributes/gcs-bucket" -H "Metadata-Flavor: Google" || echo "treecompanyimages")
GCS_PUBLIC_URL=$(curl -s -f "http://metadata.google.internal/computeMetadata/v1/instance/attributes/gcs-public-url" -H "Metadata-Flavor: Google" || echo "https://storage.googleapis.com/treecompanyimages")

echo " App starten (domein: ${ROOT_DOMAIN:-echo20.com}, bucket: ${GCS_BUCKET})..."
DB_PASSWORD="$DB_PASSWORD" \
GEMINI_API_KEY="$GEMINI_API_KEY" \
ROOT_DOMAIN="${ROOT_DOMAIN:-echo20.com}" \
APP_SUBDOMAIN="${APP_SUBDOMAIN:-treeapp}" \
GCS_BUCKET="${GCS_BUCKET}" \
GCS_PUBLIC_URL="${GCS_PUBLIC_URL}" \
sudo -E docker-compose -f docker-compose.cloud.yaml up -d --build

echo " Klaar!"