#!/bin/bash
set -euo pipefail

PROJECT_ID="integratieproject-mvp"
REGION="europe-west1"
ZONE="europe-west1-b"
INSTANCE="treeapp-db-new"
MIG_NAME="treeapp-mig"
INSTANCE_TEMPLATE="treeapp-template"
LB_NAME="treeapp-lb"
BACKEND_SERVICE="treeapp-backend"
HEALTH_CHECK="treeapp-health-check"
URL_MAP="treeapp-url-map"
TARGET_HTTPS_PROXY="treeapp-https-proxy"
FORWARDING_RULE="treeapp-https-rule"
SSL_CERT="treeapp-ssl-cert"
STATIC_IP="treeapp-ip"

echo "⚠️  Dit verwijdert ALLE cloud resources voor project: $PROJECT_ID"
echo "Ben je zeker? (yes/no)"
read -r CONFIRM
if [ "$CONFIRM" != "yes" ]; then
  echo "Geannuleerd."
  exit 0
fi

echo "🔥 Teardown gestart..."

gcloud compute forwarding-rules delete "$FORWARDING_RULE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute target-https-proxies delete "$TARGET_HTTPS_PROXY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute ssl-certificates delete "$SSL_CERT" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute url-maps delete "$URL_MAP" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute backend-services delete "$BACKEND_SERVICE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute health-checks delete "$HEALTH_CHECK" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute instance-groups managed delete "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute instance-templates delete "$INSTANCE_TEMPLATE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud compute addresses delete "$STATIC_IP" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"
gcloud sql instances delete "$INSTANCE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  (overgeslagen)"

echo "✅ Teardown voltooid!"
