#!/bin/bash
# ============================================================
# teardown.sh
# Verwijdert alle cloud resources van de echo20 omgeving
# Gebruik: bash teardown.sh
# ============================================================

set -euo pipefail

PROJECT_ID="${1:-integratieproject-mvp}"
REGION="europe-west1"
ZONE="europe-west1-b"

# Cloud SQL
INSTANCE="echo20-db"

# MIG resources
MIG_NAME="echo20-mig"

# Load balancer resources
BACKEND_SERVICE="echo20-backend"
HEALTH_CHECK="echo20-health-check"
URL_MAP="echo20-url-map"
TARGET_HTTPS_PROXY="echo20-https-proxy"
FORWARDING_RULE="echo20-https-rule"
CERT_MAP="treeapp-cert-map"
HTTP_URL_MAP="echo20-http-redirect"
TARGET_HTTP_PROXY="echo20-http-proxy"
HTTP_FORWARDING_RULE="echo20-http-rule"
STATIC_IP="echo20-ip"
SECURITY_POLICY="echo20-security-policy"

echo "  Dit verwijdert ALLE cloud resources voor project: $PROJECT_ID"
echo "Ben je zeker? (yes/no)"
read -r CONFIRM
if [ "$CONFIRM" != "yes" ]; then
  echo "Geannuleerd."
  exit 0
fi

echo " Teardown gestart..."

# Load balancer stack (verwijdert stil als niet bestaat)
# Volgorde is belangrijk: forwarding rules eerst, dan proxies, dan URL maps, dan backend, dan health check
gcloud compute forwarding-rules delete "$FORWARDING_RULE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($FORWARDING_RULE overgeslagen)"
gcloud compute forwarding-rules delete "$HTTP_FORWARDING_RULE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HTTP_FORWARDING_RULE overgeslagen)"
gcloud compute target-https-proxies delete "$TARGET_HTTPS_PROXY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($TARGET_HTTPS_PROXY overgeslagen)"
gcloud compute target-http-proxies delete "$TARGET_HTTP_PROXY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($TARGET_HTTP_PROXY overgeslagen)"
# Wildcard cert map blijft bewaard (herbruikbaar voor nieuwe deployments)
gcloud compute url-maps delete "$URL_MAP" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($URL_MAP overgeslagen)"
gcloud compute url-maps delete "$HTTP_URL_MAP" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HTTP_URL_MAP overgeslagen)"
gcloud compute backend-services update "$BACKEND_SERVICE" --security-policy="" --global --project="$PROJECT_ID" --quiet 2>/dev/null || true
gcloud compute backend-services delete "$BACKEND_SERVICE" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($BACKEND_SERVICE overgeslagen)"
gcloud compute security-policies delete "$SECURITY_POLICY" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($SECURITY_POLICY overgeslagen)"
gcloud compute health-checks delete "$HEALTH_CHECK" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($HEALTH_CHECK overgeslagen)"

# MIG eerst verwijderen (anders kunnen templates niet weg)
gcloud compute instance-groups managed delete "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($MIG_NAME overgeslagen)"

# Alle echo20-template-* templates verwijderen (er kunnen er meerdere zijn, één per branch)
echo "    Alle echo20-template-* verwijderen..."
TEMPLATES=$(gcloud compute instance-templates list --project="$PROJECT_ID" --filter="name~^echo20-template-" --format="value(name)" 2>/dev/null || echo "")
if [ -n "$TEMPLATES" ]; then
  for TEMPLATE in $TEMPLATES; do
    gcloud compute instance-templates delete "$TEMPLATE" --project="$PROJECT_ID" --quiet 2>/dev/null && echo "     $TEMPLATE verwijderd" || echo "      $TEMPLATE overgeslagen"
  done
else
  echo "    (geen templates gevonden)"
fi

# Static IP
# gcloud compute addresses delete "$STATIC_IP" --global --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($STATIC_IP overgeslagen)"

# Cloud SQL (helemaal op het einde, anders kan app niet afsluiten)
gcloud sql instances delete "$INSTANCE" --project="$PROJECT_ID" --quiet 2>/dev/null || echo "  ($INSTANCE overgeslagen)"

echo " Teardown voltooid!"
