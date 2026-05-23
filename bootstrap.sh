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
SA_NAME="echo20-vm-sa"
SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

# Base domein afleiden uit het opgegeven domein (bv. "app.youthvoice.com" → "youthvoice.com")
BASE_DOMAIN=$(echo "$DOMAIN" | awk -F. '{print $(NF-1)"."$NF}')
BASE_DOMAIN_SLUG=$(echo "$BASE_DOMAIN" | tr '.' '-')

CERT_MAP="${BASE_DOMAIN_SLUG}-cert-map"
CERT_NAME="${BASE_DOMAIN_SLUG}-wildcard-cert"
DNS_AUTH_NAME="${BASE_DOMAIN_SLUG}-dns-auth"
WILDCARD_DOMAIN="*.${BASE_DOMAIN}"

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

if ! command -v gcloud &>/dev/null; then
  echo "FOUT: gcloud CLI niet gevonden."
  echo "Installeer via: https://cloud.google.com/sdk/docs/install"
  exit 1
fi
echo "  gcloud: OK ($(gcloud --version | head -1))"

if ! command -v curl &>/dev/null; then
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

if gcloud iam service-accounts describe "$SA_EMAIL" --project="$PROJECT_ID" &>/dev/null; then
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
    --quiet 2>/dev/null
done

# Default compute SA heeft ook toegang nodig (gebruikt door VM's via startup.sh)
PROJECT_NUMBER=$(gcloud projects describe "$PROJECT_ID" --format="value(projectNumber)")
DEFAULT_COMPUTE_SA="${PROJECT_NUMBER}-compute@developer.gserviceaccount.com"
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:$DEFAULT_COMPUTE_SA" \
  --role="roles/secretmanager.secretAccessor" \
  --condition=None \
  --quiet 2>/dev/null
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:$DEFAULT_COMPUTE_SA" \
  --role="roles/cloudsql.client" \
  --condition=None \
  --quiet 2>/dev/null
echo "  IAM rollen toegewezen (cloudsql.client, secretmanager.secretAccessor)"

# ============================================================
# 5. Secrets aanmaken
# ============================================================
echo ""
echo "=== Stap 5: Secrets aanmaken ==="

secret_prompt() {
  local NAME="$1"
  local LABEL="$2"
  if gcloud secrets describe "$NAME" --project="$PROJECT_ID" &>/dev/null; then
    echo "  $NAME: bestaat al, overgeslagen"
  else
    echo ""
    echo "  Secret '$NAME' ontbreekt."
    echo "  $LABEL"
    read -rsp "  Waarde (verborgen): " VAL
    echo ""
    echo -n "$VAL" | gcloud secrets create "$NAME" --data-file=- --project="$PROJECT_ID"
    echo "  $NAME: aangemaakt"
  fi
}

secret_prompt "db-password"            "Database wachtwoord voor Cloud SQL (kies een sterk wachtwoord):"
secret_prompt "gemini-api-key"         "Google Gemini API sleutel (https://aistudio.google.com/apikey):"
secret_prompt "gitlab-deploy-username" "GitLab deploy token gebruikersnaam (GitLab > Settings > Repository > Deploy tokens):"
secret_prompt "gitlab-deploy-token"    "GitLab deploy token waarde:"

if gcloud secrets describe "sa-key" --project="$PROJECT_ID" &>/dev/null; then
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

if gcloud certificate-manager maps describe "$CERT_MAP" --project="$PROJECT_ID" &>/dev/null; then
  echo "  Cert map '$CERT_MAP': bestaat al, overgeslagen"
else
  echo "  Wildcard SSL certificaat aanmaken voor $WILDCARD_DOMAIN..."

  if ! gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" --project="$PROJECT_ID" &>/dev/null; then
    gcloud certificate-manager dns-authorizations create "$DNS_AUTH_NAME" \
      --domain="$BASE_DOMAIN" \
      --project="$PROJECT_ID" 2>/dev/null || true
  fi

  DNS_CNAME=$(gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" \
    --project="$PROJECT_ID" --format="value(dnsResourceRecord.name)")
  DNS_VALUE=$(gcloud certificate-manager dns-authorizations describe "$DNS_AUTH_NAME" \
    --project="$PROJECT_ID" --format="value(dnsResourceRecord.data)")

  DNS_SUBDOMAIN=$(echo "$DNS_CNAME" | sed "s/\\.${BASE_DOMAIN//./\\.}\\.\$//")

  echo ""
  echo "  ================================================================"
  echo "  ACTIE VEREIST: voeg dit CNAME record toe in je DNS provider"
  echo "  ================================================================"
  echo "  Type      : CNAME"
  echo "  Subdomain : $DNS_SUBDOMAIN"
  echo "  Target    : $DNS_VALUE"
  echo "  ================================================================"
  echo "  Tip: vul ALLEEN het subdomain in (zonder .${BASE_DOMAIN})"
  echo "       bv. '$DNS_SUBDOMAIN' in het 'Subdomain' veld"
  echo "       en de volledige Target waarde in het 'Target' veld"
  echo ""
  read -rp "  Druk op Enter nadat je het CNAME record hebt opgeslagen..."

  if ! gcloud certificate-manager certificates describe "$CERT_NAME" --project="$PROJECT_ID" &>/dev/null; then
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
# 7. Infrastructure opbouwen
# ============================================================
echo ""
echo "=== Stap 7: Infrastructure opbouwen ==="
echo ""

bash "$(dirname "$0")/setup.sh" main "$DOMAIN" "$PROJECT_ID"

# ============================================================
# Klaar
# ============================================================
STATIC_IP=$(gcloud compute addresses describe echo20-ip --global --project="$PROJECT_ID" --format="value(address)" 2>/dev/null || echo "<zie output hierboven>")

echo ""
echo "================================================================"
echo " Bootstrap voltooid!"
echo "================================================================"
echo ""
echo " Volgende stap: DNS A-record instellen bij je DNS provider"
echo "   $DOMAIN  ->  $STATIC_IP"
echo ""
echo " Wacht daarna 15-60 min voor SSL certificaat activatie."
echo " Check SSL status:"
echo "   gcloud certificate-manager certificates list --project=$PROJECT_ID"
echo ""
echo " Applicatie bereikbaar op: https://$DOMAIN"
echo ""
