#!/bin/bash
# ============================================================
# upgrade.sh
# Update de applicatie op de huidige VM
# Gebruik: bash upgrade.sh [BRANCH]
#   - BRANCH: optioneel, default = huidige checked-out branch
#   - Voorbeelden:
#       bash upgrade.sh                       (gebruik huidige branch)
#       bash upgrade.sh main
#       bash upgrade.sh sprint3-chatbot-survey
#
# Draait op de VM (in /opt/intergratieproject)
# ============================================================

set -euo pipefail

PROJECT_ID="integratieproject-mvp"
REPO_DIR="/opt/intergratieproject"

cd "$REPO_DIR"

# Branch bepalen: argument of huidige branch
if [ -n "${1:-}" ]; then
  BRANCH="$1"
else
  BRANCH=$(sudo git rev-parse --abbrev-ref HEAD)
fi

echo "🌿 Upgrading naar branch: $BRANCH"

echo "🔐 Secrets ophalen..."
TOKEN=$(curl -s "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token" -H "Metadata-Flavor: Google" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

DB_PASSWORD=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/db-password/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

GEMINI_API_KEY=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/gemini-api-key/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

echo "🔑 Service account key ophalen voor Cloud SQL Proxy..."
curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/sa-key/versions/latest:access" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())" | sudo tee /tmp/sa-key.json > /dev/null
sudo chmod 644 /tmp/sa-key.json

echo "📥 Laatste code ophalen (branch: $BRANCH)..."
sudo git fetch origin
sudo git checkout "$BRANCH"
sudo git pull origin "$BRANCH"

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

echo "✅ Upgrade voltooid! App draait op branch: $BRANCH"