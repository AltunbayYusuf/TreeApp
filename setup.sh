#!/bin/bash
# ============================================================
# setup.sh
# Bouwt de volledige cloud omgeving op vanaf nul
# Gebruik: bash setup.sh [BRANCH] [DOMAIN]
#   - BRANCH: optioneel, default = main
#   - DOMAIN: optioneel, bv. kdg-hogeschool.echo20.com
#             Als opgegeven: HTTPS load balancer + Google-managed SSL worden aangemaakt
#             Als weggelaten: alleen HTTP op poort 8080 (directe VM toegang)
#   - Voorbeelden:
#       bash setup.sh
#       bash setup.sh main
#       bash setup.sh main kdg-hogeschool.echo20.com
#       bash setup.sh feature/cloud-sql-proxy kdg-hogeschool.echo20.com
#
# Vereist: je bent ingelogd met gcloud en hebt rechten op project
# ============================================================

set -euo pipefail

# Branch om te deployen (default: main)
BRANCH="${1:-main}"
# Optioneel domein voor HTTPS (bv. kdg-hogeschool.echo20.com)
DOMAIN="${2:-}"

# Veilige naam voor in template-naam: vervang '/' door '-' en lowercase
BRANCH_SAFE=$(echo "$BRANCH" | tr '/' '-' | tr '[:upper:]' '[:lower:]')

PROJECT_ID="integratieproject-mvp"
REGION="europe-west1"
ZONE="europe-west1-b"

# Cloud SQL
DB_INSTANCE="echo20-db"
DB_TIER="db-f1-micro"
DB_VERSION="POSTGRES_16"
DB_NAME="TreeApp"

# MIG — template-naam bevat de branch zodat verschillende branches naast elkaar kunnen
INSTANCE_TEMPLATE="echo20-template-${BRANCH_SAFE}"
MIG_NAME="echo20-mig"
MACHINE_TYPE="e2-medium"
MIN_VMS=1
MAX_VMS=3
CPU_TARGET=0.6
COOLDOWN=600

# Load balancer resources
STATIC_IP="echo20-ip"
HEALTH_CHECK="echo20-health-check"
BACKEND_SERVICE="echo20-backend"
URL_MAP="echo20-url-map"
CERT_MAP="treeapp-cert-map"
TARGET_HTTPS_PROXY="echo20-https-proxy"
FORWARDING_RULE="echo20-https-rule"
HTTP_URL_MAP="echo20-http-redirect"
TARGET_HTTP_PROXY="echo20-http-proxy"
HTTP_FORWARDING_RULE="echo20-http-rule"

# Cloud Armor
SECURITY_POLICY="echo20-security-policy"
# Max. aantal requests per IP per minuut voordat throttling in werking treedt
RATE_LIMIT_THRESHOLD=60

echo " Setup gestart voor project: $PROJECT_ID"
echo " Deploying branch: $BRANCH"
echo " Instance template: $INSTANCE_TEMPLATE"
echo " Machine type: $MACHINE_TYPE"
if [ -n "$DOMAIN" ]; then
  echo " HTTPS domein: $DOMAIN"
fi

# ============================================================
# 1. Cloud SQL instance
# ============================================================
echo ""
echo " Stap 1: Cloud SQL Postgres aanmaken..."
if gcloud sql instances describe "$DB_INSTANCE" --project="$PROJECT_ID" &>/dev/null; then
  echo "    $DB_INSTANCE bestaat al, overgeslagen"
else
  gcloud sql instances create "$DB_INSTANCE" \
    --project="$PROJECT_ID" \
    --database-version="$DB_VERSION" \
    --tier="$DB_TIER" \
    --region="$REGION" \
    --edition=ENTERPRISE \
    --root-password="$(gcloud secrets versions access latest --secret=db-password --project=$PROJECT_ID)"

  echo "   Database $DB_NAME aanmaken..."
  gcloud sql databases create "$DB_NAME" --instance="$DB_INSTANCE" --project="$PROJECT_ID"
fi

# ============================================================
# 2. Startup script kopiëren
# ============================================================
echo ""
echo " Stap 2: Startup script kopiëren..."
cp ./startup.sh /tmp/startup.sh
echo "   Startup script opgehaald"

# ============================================================
# 3. Instance template
# ============================================================
echo ""
echo " Stap 3: Instance template aanmaken..."
if gcloud compute instance-templates describe "$INSTANCE_TEMPLATE" --project="$PROJECT_ID" &>/dev/null; then
  echo "    $INSTANCE_TEMPLATE bestaat al, overgeslagen"
else
  gcloud compute instance-templates create "$INSTANCE_TEMPLATE" \
    --project="$PROJECT_ID" \
    --machine-type="$MACHINE_TYPE" \
    --region="$REGION" \
    --network=default \
    --subnet=default \
    --image-family=debian-12 \
    --image-project=debian-cloud \
    --scopes=cloud-platform \
    --tags=http-server,https-server \
    --metadata-from-file=startup-script=/tmp/startup.sh \
    --metadata=deploy-branch="$BRANCH"
fi

# ============================================================
# 4. MIG (Managed Instance Group)
# ============================================================
echo ""
echo " Stap 4: Managed Instance Group aanmaken..."
if gcloud compute instance-groups managed describe "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" &>/dev/null; then
  echo "    $MIG_NAME bestaat al"
  echo "   Template wisselen naar $INSTANCE_TEMPLATE en VM's vervangen..."
  gcloud compute instance-groups managed set-instance-template "$MIG_NAME" \
    --zone="$ZONE" \
    --project="$PROJECT_ID" \
    --template="$INSTANCE_TEMPLATE"

  gcloud compute instance-groups managed rolling-action replace "$MIG_NAME" \
    --zone="$ZONE" \
    --project="$PROJECT_ID" \
    --max-unavailable=1
  echo "   Rolling replace gestart"
else
  gcloud compute instance-groups managed create "$MIG_NAME" \
    --project="$PROJECT_ID" \
    --zone="$ZONE" \
    --template="$INSTANCE_TEMPLATE" \
    --size=1 \
    --base-instance-name=echo20
fi

# ============================================================
# 5. Autoscaling
# ============================================================
echo ""
echo " Stap 5: Autoscaling configureren..."
gcloud compute instance-groups managed set-autoscaling "$MIG_NAME" \
  --zone="$ZONE" \
  --project="$PROJECT_ID" \
  --min-num-replicas="$MIN_VMS" \
  --max-num-replicas="$MAX_VMS" \
  --target-cpu-utilization="$CPU_TARGET" \
  --cool-down-period="$COOLDOWN"

# ============================================================
# 6. Firewall regel voor poort 8080
# ============================================================
echo ""
echo " Stap 6: Firewall regels..."
if gcloud compute firewall-rules describe allow-http-8080 --project="$PROJECT_ID" &>/dev/null; then
  echo "    allow-http-8080 bestaat al, overgeslagen"
else
  gcloud compute firewall-rules create allow-http-8080 \
    --project="$PROJECT_ID" \
    --direction=INGRESS \
    --action=ALLOW \
    --rules=tcp:8080 \
    --source-ranges=0.0.0.0/0 \
    --target-tags=http-server
fi

# ============================================================
# 7. Statisch IP adres
# ============================================================
echo ""
echo " Stap 7: Statisch IP adres reserveren..."
if gcloud compute addresses describe "$STATIC_IP" --global --project="$PROJECT_ID" &>/dev/null; then
  echo "    $STATIC_IP bestaat al, overgeslagen"
else
  gcloud compute addresses create "$STATIC_IP" \
    --global \
    --project="$PROJECT_ID"
fi
STATIC_IP_ADDRESS=$(gcloud compute addresses describe "$STATIC_IP" --global --project="$PROJECT_ID" --format="value(address)")
echo "   Statisch IP: $STATIC_IP_ADDRESS"

# ============================================================
# 8. Health check + named ports voor de load balancer
# ============================================================
echo ""
echo "  Stap 8: Health check aanmaken..."
if gcloud compute health-checks describe "$HEALTH_CHECK" --project="$PROJECT_ID" &>/dev/null; then
  echo "    $HEALTH_CHECK bestaat al, overgeslagen"
else
  MSYS2_ARG_CONV_EXCL="--request-path" gcloud compute health-checks create http "$HEALTH_CHECK" \
    --port=8080 \
    --request-path=/health \
    --check-interval=10s \
    --timeout=5s \
    --healthy-threshold=2 \
    --unhealthy-threshold=3 \
    --project="$PROJECT_ID"
fi

echo "   Named ports instellen op MIG..."
gcloud compute instance-groups managed set-named-ports "$MIG_NAME" \
  --named-ports=http:8080 \
  --zone="$ZONE" \
  --project="$PROJECT_ID"

# ============================================================
# 9. Backend service (met session affinity voor sticky sessions)
# ============================================================
echo ""
echo "  Stap 9: Backend service aanmaken..."
if gcloud compute backend-services describe "$BACKEND_SERVICE" --global --project="$PROJECT_ID" &>/dev/null; then
  echo "    $BACKEND_SERVICE bestaat al, overgeslagen"
else
  gcloud compute backend-services create "$BACKEND_SERVICE" \
    --protocol=HTTP \
    --port-name=http \
    --health-checks="$HEALTH_CHECK" \
    --session-affinity=GENERATED_COOKIE \
    --affinity-cookie-ttl=86400 \
    --global \
    --project="$PROJECT_ID"

  gcloud compute backend-services add-backend "$BACKEND_SERVICE" \
    --instance-group="$MIG_NAME" \
    --instance-group-zone="$ZONE" \
    --global \
    --project="$PROJECT_ID"
  echo "   Backend service aangemaakt met session affinity (cookie, 24u TTL)"
fi

# ============================================================
# 10. Cloud Armor security policy (rate limiting + DDoS bescherming)
# ============================================================
echo ""
echo "  Stap 10: Cloud Armor security policy aanmaken..."
if gcloud compute security-policies describe "$SECURITY_POLICY" --project="$PROJECT_ID" &>/dev/null; then
  echo "    $SECURITY_POLICY bestaat al, overgeslagen"
else
  gcloud compute security-policies create "$SECURITY_POLICY" \
    --project="$PROJECT_ID" \
    --description="Rate limiting en DDoS bescherming voor TreeApp"

  # Rate limiting: throttle bij meer dan $RATE_LIMIT_THRESHOLD requests/minuut per IP
  gcloud compute security-policies rules create 1000 \
    --security-policy="$SECURITY_POLICY" \
    --project="$PROJECT_ID" \
    --action=throttle \
    --src-ip-ranges="*" \
    --rate-limit-threshold-count="$RATE_LIMIT_THRESHOLD" \
    --rate-limit-threshold-interval-sec=60 \
    --conform-action=allow \
    --exceed-action=deny-429 \
    --enforce-on-key=IP

  echo "   Security policy aangemaakt (throttle: $RATE_LIMIT_THRESHOLD req/min per IP)"
fi

echo "   Security policy koppelen aan backend service..."
gcloud compute backend-services update "$BACKEND_SERVICE" \
  --security-policy="$SECURITY_POLICY" \
  --global \
  --project="$PROJECT_ID"
echo "   Cloud Armor actief op $BACKEND_SERVICE"

# ============================================================
# 11. HTTPS Load Balancer (alleen als DOMAIN opgegeven)
# ============================================================
echo ""
if [ -n "$DOMAIN" ]; then
  echo " Stap 11: HTTPS load balancer aanmaken voor $DOMAIN..."

  # URL map voor HTTPS verkeer
  if gcloud compute url-maps describe "$URL_MAP" --project="$PROJECT_ID" &>/dev/null; then
    echo "    URL map bestaat al, overgeslagen"
  else
    gcloud compute url-maps create "$URL_MAP" \
      --default-service="$BACKEND_SERVICE" \
      --project="$PROJECT_ID"
  fi

  # Wildcard SSL via Certificate Manager (dekt automatisch elk *.echo20.com subdomein)
  # De cert map 'treeapp-cert-map' bevat een ACTIEF wildcard-cert voor *.echo20.com
  # Nieuw subdomein toevoegen = alleen DNS-record aanmaken, niets anders

  # HTTPS target proxy
  if gcloud compute target-https-proxies describe "$TARGET_HTTPS_PROXY" --project="$PROJECT_ID" &>/dev/null; then
    echo "    HTTPS proxy bestaat al, overgeslagen"
  else
    gcloud compute target-https-proxies create "$TARGET_HTTPS_PROXY" \
      --url-map="$URL_MAP" \
      --certificate-map="$CERT_MAP" \
      --project="$PROJECT_ID"
  fi

  # HTTPS forwarding rule (poort 443)
  if gcloud compute forwarding-rules describe "$FORWARDING_RULE" --global --project="$PROJECT_ID" &>/dev/null; then
    echo "    HTTPS forwarding rule bestaat al, overgeslagen"
  else
    gcloud compute forwarding-rules create "$FORWARDING_RULE" \
      --target-https-proxy="$TARGET_HTTPS_PROXY" \
      --ports=443 \
      --global \
      --address="$STATIC_IP" \
      --project="$PROJECT_ID"
  fi

  # HTTP → HTTPS redirect (aparte URL map met redirect actie)
  if gcloud compute url-maps describe "$HTTP_URL_MAP" --project="$PROJECT_ID" &>/dev/null; then
    echo "    HTTP redirect URL map bestaat al, overgeslagen"
  else
    cat > /tmp/http-redirect.yaml <<"YAMLEOF"
name: echo20-http-redirect
defaultUrlRedirect:
  redirectResponseCode: MOVED_PERMANENTLY_DEFAULT
  httpsRedirect: true
YAMLEOF
    gcloud compute url-maps import "$HTTP_URL_MAP" \
      --source=/tmp/http-redirect.yaml \
      --global \
      --project="$PROJECT_ID"
  fi

  if gcloud compute target-http-proxies describe "$TARGET_HTTP_PROXY" --project="$PROJECT_ID" &>/dev/null; then
    echo "    HTTP proxy bestaat al, overgeslagen"
  else
    gcloud compute target-http-proxies create "$TARGET_HTTP_PROXY" \
      --url-map="$HTTP_URL_MAP" \
      --project="$PROJECT_ID"
  fi

  if gcloud compute forwarding-rules describe "$HTTP_FORWARDING_RULE" --global --project="$PROJECT_ID" &>/dev/null; then
    echo "    HTTP forwarding rule bestaat al, overgeslagen"
  else
    gcloud compute forwarding-rules create "$HTTP_FORWARDING_RULE" \
      --target-http-proxy="$TARGET_HTTP_PROXY" \
      --ports=80 \
      --global \
      --address="$STATIC_IP" \
      --project="$PROJECT_ID"
  fi

  echo ""
  echo " HTTPS load balancer geconfigureerd!"
  echo "    HTTPS URL : https://$DOMAIN"
  echo "    HTTP → HTTPS redirect actief"
  echo "    Session affinity: GENERATED_COOKIE (24u)"
  echo "    DNS instelling vereist: $DOMAIN → $STATIC_IP_ADDRESS"
  echo "    SSL provisioning kan 15-60 min duren na DNS koppeling"
else
  echo "  Stap 11: Geen DOMAIN opgegeven — HTTPS load balancer overgeslagen"
  echo "    Gebruik: bash setup.sh $BRANCH <jouw-domein.com>"
  echo "    Direct toegankelijk via VM IP op poort 8080"
fi

echo ""
echo " Setup voltooid voor branch: $BRANCH"
echo ""
echo " Wacht 5-8 minuten tot de VM volledig opgestart is."
echo " Check status met:"
echo "   gcloud compute instance-groups managed list-instances $MIG_NAME --zone=$ZONE --project=$PROJECT_ID"
echo ""
echo " Vind het externe IP:"
echo "   gcloud compute instances list --filter=\"name~echo20\" --format=\"value(name,EXTERNAL_IP)\""
