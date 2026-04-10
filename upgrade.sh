#!/bin/bash
# ============================================================
# upgrade.sh
# Update de applicatie zonder downtime
# Gebruik: bash upgrade.sh
# ============================================================

set -euo pipefail

PROJECT_ID="integratieproject-mvp"

echo "🔐 Secrets ophalen..."
TOKEN=$(curl -s "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token" -H "Metadata-Flavor: Google" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

DB_PASSWORD=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/db-password/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

GEMINI_API_KEY=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/gemini-api-key/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

echo "📥 Laatste code ophalen..."
cd ~/intergratieproject
sudo git pull origin main

echo "🔨 Nieuwe image bouwen..."
DB_PASSWORD="$DB_PASSWORD" \
GEMINI_API_KEY="$GEMINI_API_KEY" \
sudo -E docker-compose -f docker-compose.cloud.yaml build

echo "🔄 Container vervangen zonder downtime..."
DB_PASSWORD="$DB_PASSWORD" \
GEMINI_API_KEY="$GEMINI_API_KEY" \
sudo -E docker-compose -f docker-compose.cloud.yaml up -d --no-deps web

echo "🧹 Oude images opruimen..."
sudo docker image prune -f

echo "✅ Upgrade voltooid! App draait op nieuwe versie."
