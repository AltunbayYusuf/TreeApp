#!/bin/bash
# ============================================================
# bootstrap.sh
# Deployt de volledige TreeApp omgeving vanaf nul op GCP.
# Gebruik: bash bootstrap.sh <DOMAIN> [PROJECT_ID]
# Voorbeeld: bash bootstrap.sh kdg-hogeschool.echo20.com
#            bash bootstrap.sh test.echo20.com mijn-nieuw-project
#
# Vereisten (zie README.md):
#   - gcloud CLI geinstalleerd
#   - GCP project met actief billing account
#   - Toegang tot DNS provider van het opgegeven domein
# ============================================================

set -euo pipefail

DOMAIN="${1:-}"
PROJECT_ID="${2:-integratieproject-mvp}"
SA_NAME="treeapp-vm-sa"
SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

# Bron-project voor automatische secret-overname en Cloud DNS beheer.
# Wijzig deze waarden als je de demo vanuit een ander project wil runnen.
SECRETS_SOURCE_PROJECT="${SECRETS_SOURCE_PROJECT:-integratieproject-mvp}"
DNS_ZONE_PROJECT="${DNS_ZONE_PROJECT:-integratieproject-mvp}"
DNS_ZONE_NAME="${DNS_ZONE_NAME:-test-echo20}"

# Base domein afleiden uit het opgegeven domein (eerste label strippen)
# bv. "kdg-hogeschool.echo20.com" -> "echo20.com"
# bv. "kdg-hogeschool.test.echo20.com" -> "test.echo20.com" (voor demo-deployments)
BASE_DOMAIN="${DOMAIN#*.}"
BASE_DOMAIN_SLUG=$(echo "$BASE_DOMAIN" | tr '.' '-')

CERT_MAP="${BASE_DOMAIN_SLUG}-cert-map"
CERT_NAME="${BASE_DOMAIN_SLUG}-wildcard-cert"
DNS_AUTH_NAME="${BASE_DOMAIN_SLUG}-dns-auth"
WILDCARD_DOMAIN="*.${BASE_DOMAIN}"
GCS_BUCKET="${PROJECT_ID}-images"
GCS_PUBLIC_URL="https://storage.googleapis.com/${GCS_BUCKET}"

# ============================================================
# Helpers — automatische DNS + secret overname + retry
# ============================================================

# Voer een commando uit met automatische retry bij netwerk- of API-fouten.
# 3 pogingen met exponentiele backoff (5s -> 10s -> 20s).
# Gebruik alleen voor calls waarvan we succes verwachten (niet voor "exists" checks).
retry() {
  local MAX=3 DELAY=5 ATTEMPT=1
  while [ $ATTEMPT -le $MAX ]; do
    if "$@"; then
      return 0
    fi
    if [ $ATTEMPT -lt $MAX ]; then
      echo "  (retry) commando faalde, opnieuw na ${DELAY}s (poging $ATTEMPT/$MAX)..." >&2
      sleep $DELAY
      DELAY=$((DELAY * 2))
    fi
    ATTEMPT=$((ATTEMPT + 1))
  done
  echo "  (retry) commando definitief gefaald na $MAX pogingen" >&2
  return 1
}

# Zet (of update) een DNS record in de Cloud DNS managed zone.
# Gebruikt door bootstrap voor CNAME (DNS authorization) en wildcard A-record.
# Args: NAME (FQDN met trailing dot), TYPE (CNAME/A), TTL, DATA
dns_record_set() {
  local NAME="$1" TYPE="$2" TTL="$3" DATA="$4"
  local EXISTING
  EXISTING=$(gcloud dns record-sets list \
    --zone="$DNS_ZONE_NAME" \
    --project="$DNS_ZONE_PROJECT" \
    --name="$NAME" \
    --type="$TYPE" \
    --format="value(rrdatas)" 2>/dev/null || echo "")

  if [ "$EXISTING" = "$DATA" ]; then
    echo "  DNS $TYPE $NAME: al correct"
    return
  fi

  if [ -n "$EXISTING" ]; then
    gcloud dns record-sets delete "$NAME" \
      --zone="$DNS_ZONE_NAME" \
      --type="$TYPE" \
      --project="$DNS_ZONE_PROJECT" \
      --quiet >/dev/null
  fi

  gcloud dns record-sets create "$NAME" \
    --zone="$DNS_ZONE_NAME" \
    --type="$TYPE" \
    --ttl="$TTL" \
    --rrdatas="$DATA" \
    --project="$DNS_ZONE_PROJECT" >/dev/null
  echo "  DNS $TYPE $NAME -> $DATA"
}

# Zorg dat een secret in het target project bestaat door 'm uit het source project te kopieren.
# Gebruikt om db-password, gemini-api-key en gitlab-deploy-* automatisch over te nemen
# uit integratieproject-mvp zonder dat de gebruiker iets hoeft in te tikken.
secret_ensure() {
  local NAME="$1"
  if gcloud secrets describe "$NAME" --project="$PROJECT_ID" >/dev/null 2>&1; then
    echo "  $NAME: bestaat al in $PROJECT_ID"
    return
  fi

  if [ "$PROJECT_ID" = "$SECRETS_SOURCE_PROJECT" ]; then
    echo "FOUT: secret '$NAME' ontbreekt in $PROJECT_ID (bron = target, geen kopie mogelijk)"
    exit 1
  fi

  if ! gcloud secrets describe "$NAME" --project="$SECRETS_SOURCE_PROJECT" >/dev/null 2>&1; then
    echo "FOUT: secret '$NAME' bestaat ook niet in bron-project $SECRETS_SOURCE_PROJECT"
    echo "Maak 'm daar eenmalig aan, of zet SECRETS_SOURCE_PROJECT op een ander project."
    exit 1
  fi

  gcloud secrets versions access latest \
    --secret="$NAME" \
    --project="$SECRETS_SOURCE_PROJECT" 2>/dev/null | \
    gcloud secrets create "$NAME" \
      --data-file=- \
      --project="$PROJECT_ID" >/dev/null
  echo "  $NAME: gekopieerd uit $SECRETS_SOURCE_PROJECT"
}

if [ -z "$DOMAIN" ]; then
  echo "Gebruik: bash bootstrap.sh <DOMAIN>"
  echo "Voorbeeld: bash bootstrap.sh kdg-hogeschool.echo20.com"
  exit 1
fi

# ============================================================
# 1. Prerequisites
# ============================================================
echo ""
echo "=== Stap 1: Prerequisites controleren ==="

if ! command -v gcloud >/dev/null 2>&1; then
  echo "FOUT: gcloud CLI niet gevonden."
  echo "Installeer via: https://cloud.google.com/sdk/docs/install"
  exit 1
fi
echo "  gcloud: OK ($(gcloud --version | head -1))"

if ! command -v curl >/dev/null 2>&1; then
  echo "FOUT: curl niet gevonden. Installeer curl en probeer opnieuw."
  exit 1
fi
echo "  curl: OK"

# ============================================================
# 2. Authenticatie
# ============================================================
echo ""
echo "=== Stap 2: Authenticatie ==="

ACTIVE_ACCOUNT=$(gcloud auth list --filter=status:ACTIVE --format="value(account)" 2>/dev/null | head -1 || echo "")
if [ -z "$ACTIVE_ACCOUNT" ]; then
  echo "Niet ingelogd. Browser opent voor authenticatie..."
  gcloud auth login
  ACTIVE_ACCOUNT=$(gcloud auth list --filter=status:ACTIVE --format="value(account)" | head -1)
fi
echo "  Ingelogd als: $ACTIVE_ACCOUNT"

gcloud config set project "$PROJECT_ID"
echo "  Project: $PROJECT_ID"

# Controleer of de Cloud DNS managed zone bestaat (vereist voor autonome DNS-setup).
if ! gcloud dns managed-zones describe "$DNS_ZONE_NAME" \
    --project="$DNS_ZONE_PROJECT" >/dev/null 2>&1; then
  echo ""
  echo "FOUT: Cloud DNS zone '$DNS_ZONE_NAME' bestaat niet in project '$DNS_ZONE_PROJECT'."
  echo ""
  echo "Eenmalige setup (één keer per provider):"
  echo "  1. Maak de managed zone aan:"
  echo "       gcloud dns managed-zones create $DNS_ZONE_NAME \\"
  echo "         --dns-name=${BASE_DOMAIN}. \\"
  echo "         --description='Auto-DNS voor deployments' \\"
  echo "         --project=$DNS_ZONE_PROJECT"
  echo ""
  echo "  2. Haal de NS records op:"
  echo "       gcloud dns managed-zones describe $DNS_ZONE_NAME \\"
  echo "         --project=$DNS_ZONE_PROJECT --format='value(nameServers)'"
  echo ""
  echo "  3. Voeg die 4 NS records toe bij OVH voor subdomein '${BASE_DOMAIN%%.*}'."
  echo ""
  echo "Daarna draait elke nieuwe deployment volledig autonoom."
  exit 1
fi
echo "  Cloud DNS zone: $DNS_ZONE_NAME (project: $DNS_ZONE_PROJECT)"

# Controleer of het bron-project bereikbaar is voor secret-overname.
if [ "$PROJECT_ID" != "$SECRETS_SOURCE_PROJECT" ]; then
  if ! gcloud projects describe "$SECRETS_SOURCE_PROJECT" >/dev/null 2>&1; then
    echo "FOUT: geen toegang tot bron-project '$SECRETS_SOURCE_PROJECT' (voor secret-overname)"
    exit 1
  fi
  echo "  Bron-project secrets: $SECRETS_SOURCE_PROJECT"
fi

# ============================================================
# 3. APIs inschakelen
# ============================================================
echo ""
echo "=== Stap 3: GCP APIs inschakelen ==="

APIS=(
  "compute.googleapis.com"
  "sqladmin.googleapis.com"
  "secretmanager.googleapis.com"
  "certificatemanager.googleapis.com"
  "iam.googleapis.com"
  "storage.googleapis.com"
  "aiplatform.googleapis.com"
)

for API in "${APIS[@]}"; do
  if gcloud services list --enabled --filter="name:${API}" --project="$PROJECT_ID" --format="value(name)" 2>/dev/null | grep -q "$API"; then
    echo "  $API: al actief"
  else
    echo "  $API inschakelen..."
    gcloud services enable "$API" --project="$PROJECT_ID"
    echo "  $API: ingeschakeld"
  fi
done

# ============================================================
# 4. Service account voor de VM's
# ============================================================
echo ""
echo "=== Stap 4: Service account aanmaken ==="

if gcloud iam service-accounts describe "$SA_EMAIL" --project="$PROJECT_ID" >/dev/null 2>&1; then
  echo "  $SA_EMAIL: bestaat al, overgeslagen"
else
  gcloud iam service-accounts create "$SA_NAME" \
    --display-name="Echo20 VM Service Account" \
    --project="$PROJECT_ID"
  echo "  Service account aangemaakt: $SA_EMAIL"
fi

for ROLE in "roles/cloudsql.client" "roles/secretmanager.secretAccessor"; do
  gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:$SA_EMAIL" \
    --role="$ROLE" \
    --condition=None \
    --quiet >/dev/null || true
done

# Default compute SA heeft ook toegang nodig (gebruikt door VM's via startup.sh)
PROJECT_NUMBER=$(retry gcloud projects describe "$PROJECT_ID" --format="value(projectNumber)")
DEFAULT_COMPUTE_SA="${PROJECT_NUMBER}-compute@developer.gserviceaccount.com"
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:$DEFAULT_COMPUTE_SA" \
  --role="roles/secretmanager.secretAccessor" \
  --condition=None \
  --quiet >/dev/null || true
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:$DEFAULT_COMPUTE_SA" \
  --role="roles/cloudsql.client" \
  --condition=None \
  --quiet >/dev/null || true
echo "  IAM rollen toegewezen (cloudsql.client, secretmanager.secretAccessor)"

# ============================================================
# 5. Secrets aanmaken
# ============================================================
echo ""
echo "=== Stap 5: Secrets overnemen uit bron-project ==="

secret_ensure "db-password"
secret_ensure "gemini-api-key"
secret_ensure "gitlab-deploy-username"
secret_ensure "gitlab-deploy-token"

if gcloud secrets describe "sa-key" --project="$PROJECT_ID" >/dev/null 2>&1; then
  echo "  sa-key: bestaat al, overgeslagen"
else
  echo "  Service account key aanmaken en opslaan als secret..."
  gcloud iam service-accounts keys create /tmp/sa-key-bootstrap.json \
    --iam-account="$SA_EMAIL" \
    --project="$PROJECT_ID"
  gcloud secrets create "sa-key" \
    --data-file=/tmp/sa-key-bootstrap.json \
    --project="$PROJECT_ID"
  rm -f /tmp/sa-key-bootstrap.json
  echo "  sa-key: aangemaakt"
fi

# ============================================================
# 6. Wildcard SSL certificaat (Certificate Manager)
# ============================================================
echo ""
echo "=== Stap 6: SSL certificaat controleren ==="

if gcloud certificate-manager maps describe "$CERT_MAP" --project="$PROJECT_ID" >/dev/null 2>&1; then
  echo "  Cert map '$CERT_MAP': bestaat al, overgeslagen"
else
  echo "  Wildcard SSL certificaat aanmaken voor $WILDCARD_DOMAIN..."

  if gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" \
      --project="$PROJECT_ID" >/dev/null 2>&1; then
    echo "  DNS autorisatie $DNS_AUTH_NAME: bestaat al"
  else
    if ! gcloud certificate-manager dns-authorizations create "$DNS_AUTH_NAME" \
        --domain="$BASE_DOMAIN" \
        --project="$PROJECT_ID" >/dev/null 2>&1; then
      # Aanmaken mislukt: domein heeft al een autorisatie onder een andere naam
      EXISTING_AUTH=$(gcloud certificate-manager dns-authorizations list \
        --project="$PROJECT_ID" --format="value(name)" 2>/dev/null | head -1)
      if [ -n "$EXISTING_AUTH" ]; then
        DNS_AUTH_NAME="$EXISTING_AUTH"
        echo "  Bestaande DNS autorisatie hergebruikt: $DNS_AUTH_NAME"
      else
        echo "FOUT: Kon geen DNS autorisatie aanmaken of vinden voor $BASE_DOMAIN"
        exit 1
      fi
    fi
  fi

  DNS_CNAME=$(gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" \
    --project="$PROJECT_ID" --format="value(dnsResourceRecord.name)")
  DNS_VALUE=$(gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" \
    --project="$PROJECT_ID" --format="value(dnsResourceRecord.data)")

  # Zorg dat name en data eindigen op een trailing dot (vereist door Cloud DNS)
  DNS_CNAME_FQDN="${DNS_CNAME%.}."
  DNS_VALUE_FQDN="${DNS_VALUE%.}."

  echo "  CNAME record automatisch toevoegen in Cloud DNS..."
  dns_record_set "$DNS_CNAME_FQDN" "CNAME" 300 "$DNS_VALUE_FQDN"

  if ! gcloud certificate-manager certificates describe "$CERT_NAME" --project="$PROJECT_ID" >/dev/null 2>&1; then
    gcloud certificate-manager certificates create "$CERT_NAME" \
      --domains="$WILDCARD_DOMAIN" \
      --dns-authorizations="$DNS_AUTH_NAME" \
      --project="$PROJECT_ID"
  fi

  gcloud certificate-manager maps create "$CERT_MAP" --project="$PROJECT_ID"

  gcloud certificate-manager maps entries create "${BASE_DOMAIN_SLUG}-wildcard-entry" \
    --map="$CERT_MAP" \
    --certificates="$CERT_NAME" \
    --hostname="$WILDCARD_DOMAIN" \
    --project="$PROJECT_ID"

  echo "  Certificaat ingediend. Wordt ACTIVE na DNS validatie (10-30 min)."
fi

# ============================================================
# 7. GCS bucket aanmaken voor afbeeldingen
# ============================================================
echo ""
echo "=== Stap 7: GCS bucket aanmaken ==="

if gsutil ls -p "$PROJECT_ID" "gs://${GCS_BUCKET}" >/dev/null 2>&1; then
  echo "  Bucket gs://${GCS_BUCKET}: bestaat al, overgeslagen"
else
  gsutil mb -p "$PROJECT_ID" -l "europe-west1" "gs://${GCS_BUCKET}"
  gsutil iam ch allUsers:objectViewer "gs://${GCS_BUCKET}"
  echo "  Bucket aangemaakt: gs://${GCS_BUCKET} (publiek leesbaar)"
fi

# ============================================================
# 8. Infrastructure opbouwen
# ============================================================
echo ""
echo "=== Stap 8: Infrastructure opbouwen ==="
echo ""

bash "$(dirname "$0")/setup.sh" main "$DOMAIN" "$PROJECT_ID" "$GCS_BUCKET" "$GCS_PUBLIC_URL"

# ============================================================
# 9. Wildcard A-record automatisch toevoegen
# ============================================================
echo ""
echo "=== Stap 9: Wildcard A-record toevoegen ==="
STATIC_IP=$(retry gcloud compute addresses describe treeapp-ip --global --project="$PROJECT_ID" --format="value(address)" 2>/dev/null || echo "")

if [ -z "$STATIC_IP" ]; then
  echo "WAARSCHUWING: kon het statisch IP niet ophalen, A-record niet aangemaakt"
else
  dns_record_set "*.${BASE_DOMAIN}." "A" 300 "$STATIC_IP"
fi

# ============================================================
# Klaar
# ============================================================
echo ""
echo "================================================================"
echo " Bootstrap voltooid — volledig autonoom!"
echo "================================================================"
echo ""
echo " Statisch IP : $STATIC_IP"
echo " Wildcard A  : *.${BASE_DOMAIN} -> $STATIC_IP (auto via Cloud DNS)"
echo " CNAME       : DNS authorization auto-toegevoegd (zie stap 6)"
echo ""
echo " SSL certificaat valideert nu in de achtergrond (15-60 min)."
echo " Check SSL status:"
echo "   gcloud certificate-manager certificates list --project=$PROJECT_ID"
echo ""
echo " Applicatie wordt bereikbaar op: https://$DOMAIN"
echo ""
