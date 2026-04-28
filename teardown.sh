#!/bin/bash
# ============================================================
# teardown.sh
# Verwijdert alle cloud resources van de TreeApp omgeving
# Gebruik: bash teardown.sh
# ============================================================

set -euo pipefail

PROJECT_ID="integratieproject-mvp"
REGION="europe-west1"
ZONE="europe-west1-b"

# Cloud SQL
INSTANCE="treeapp-db-new"

# MIG resources
MIG_NAME="treeapp-mig"

# Load balancer resources
BACKEND_SERVICE="treeapp-backend"
HEALTH_CHECK="treeapp-health-check"
URL_MAP="treeapp-url-map"
TARGET_HTTPS_PROXY="treeapp-https-proxy"
FORWARDING_RULE="treeapp-https-rule"
SSL_CERT="treeapp-ssl-cert"
HTTP_URL_MAP="treeapp-http-redirect"
TARGET_HTTP_PROXY="treeapp-http-proxy"
HTTP_FORWARDING_RULE="treeapp-http-rule"
STATIC_IP="treeapp-ip"

echo "⚠️  Dit verwijdert ALLE cloud resources voor project: $PROJECT_ID"
echo "Ben je zeker? (yes/no)"
read -r CONFIRM
if [ "$CONFIRM" != "yes" ]; then
  echo "Geannuleerd."
  exit 0
fi

echo "🔥 Teardown gestart..."

# Load balancer stack (verwijdert stil als niet bestaat)
# Volgorde is belangrijk: forwarding rules eerst, dan proxies, dan URL maps, dan backend, dan health check
gcloud compute forwarding-rules delete "$FORWARDING_RULE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($FORWARDING_RULE overgeslagen)"
gcloud compute forwarding-rules delete "$HTTP_FORWARDING_RULE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HTTP_FORWARDING_RULE overgeslagen)"
gcloud compute target-https-proxies delete "$TARGET_HTTPS_PROXY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($TARGET_HTTPS_PROXY overgeslagen)"
gcloud compute target-http-proxies delete "$TARGET_HTTP_PROXY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($TARGET_HTTP_PROXY overgeslagen)"
gcloud compute ssl-certificates delete "$SSL_CERT" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($SSL_CERT overgeslagen)"
gcloud compute url-maps delete "$URL_MAP" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($URL_MAP overgeslagen)"
gcloud compute url-maps delete "$HTTP_URL_MAP" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HTTP_URL_MAP overgeslagen)"
gcloud compute backend-services delete "$BACKEND_SERVICE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($BACKEND_SERVICE overgeslagen)"
gcloud compute health-checks delete "$HEALTH_CHECK" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HEALTH_CHECK overgeslagen)"

# MIG eerst verwijderen (anders kunnen templates niet weg)
gcloud compute instance-groups managed delete "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($MIG_NAME overgeslagen)"

# Alle treeapp-template-* templates verwijderen (er kunnen er meerdere zijn, één per branch)
echo "  🗑️  Alle treeapp-template-* verwijderen..."
TEMPLATES=$(gcloud compute instance-templates list --project="$PROJECT_ID" --filter="name~^treeapp-template-" --format="value(name)" 2>/dev/null || echo "")
if [ -n "$TEMPLATES" ]; then
  for TEMPLATE in $TEMPLATES; do
    gcloud compute instance-templates delete "$TEMPLATE" --project="$PROJECT_ID" --quiet 2>/dev/null && echo "    ✅ $TEMPLATE verwijderd" || echo "    ⏭️  $TEMPLATE overgeslagen"
  done
else
  echo "    (geen templates gevonden)"
fi

# Static IP
gcloud compute addresses delete "$STATIC_IP" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($STATIC_IP overgeslagen)"

# Cloud SQL (helemaal op het einde, anders kan app niet afsluiten)
gcloud sql instances delete "$INSTANCE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($INSTANCE overgeslagen)"

echo "✅ Teardown voltooid!"