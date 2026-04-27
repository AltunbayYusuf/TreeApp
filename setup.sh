#!/bin/bash
# ============================================================
# setup.sh
# Bouwt de volledige cloud omgeving op vanaf nul
# Gebruik: bash setup.sh [BRANCH]
#   - BRANCH: optioneel, default = main
#   - Voorbeelden:
#       bash setup.sh
#       bash setup.sh main
#       bash setup.sh sprint3-chatbot-survey
#       bash setup.sh feature/nieuwe-feature
#
# Vereist: je bent ingelogd met gcloud en hebt rechten op project
# ============================================================

set -euo pipefail

# Branch om te deployen (default: main)
BRANCH="${1:-main}"

# Veilige naam voor in template-naam: vervang '/' door '-' en lowercase
BRANCH_SAFE=$(echo "$BRANCH" | tr '/' '-' | tr '[:upper:]' '[:lower:]')

PROJECT_ID="integratieproject-mvp"
REGION="europe-west1"
ZONE="europe-west1-b"

# Cloud SQL
DB_INSTANCE="treeapp-db-new"
DB_TIER="db-f1-micro"
DB_VERSION="POSTGRES_16"
DB_NAME="TreeApp"

# MIG — template-naam bevat de branch zodat verschillende branches naast elkaar kunnen
INSTANCE_TEMPLATE="treeapp-template-${BRANCH_SAFE}"
MIG_NAME="treeapp-mig"
MACHINE_TYPE="e2-small"
MIN_VMS=1
MAX_VMS=3
CPU_TARGET=0.6
COOLDOWN=600

echo "🚀 Setup gestart voor project: $PROJECT_ID"
echo "🌿 Deploying branch: $BRANCH"
echo "📋 Instance template: $INSTANCE_TEMPLATE"
echo "💻 Machine type: $MACHINE_TYPE"

# ============================================================
# 1. Cloud SQL instance
# ============================================================
echo ""
echo "📦 Stap 1: Cloud SQL Postgres aanmaken..."
if gcloud sql instances describe "$DB_INSTANCE" --project="$PROJECT_ID" &>/dev/null; then
  echo "  ⏭️  $DB_INSTANCE bestaat al, overgeslagen"
else
  gcloud sql instances create "$DB_INSTANCE" \
    --project="$PROJECT_ID" \
    --database-version="$DB_VERSION" \
    --tier="$DB_TIER" \
    --region="$REGION" \
    --edition=ENTERPRISE \
    --root-password="$(gcloud secrets versions access latest --secret=db-password --project=$PROJECT_ID)"

  echo "  📋 Database $DB_NAME aanmaken..."
  gcloud sql databases create "$DB_NAME" --instance="$DB_INSTANCE" --project="$PROJECT_ID"
fi

# ============================================================
# 2. Startup script downloaden
# ============================================================
echo ""
echo "📥 Stap 2: Startup script downloaden uit GitLab (branch: $BRANCH)..."
GIT_USER=$(gcloud secrets versions access latest --secret=gitlab-deploy-username --project="$PROJECT_ID")
GIT_TOKEN=$(gcloud secrets versions access latest --secret=gitlab-deploy-token --project="$PROJECT_ID")

rm -rf /tmp/tree-setup-repo
git clone --depth 1 --branch "$BRANCH" \
  "https://$GIT_USER:$GIT_TOKEN@gitlab.com/kdg-ti/integratieproject-1/2526/20_echo/intergratieproject.git" \
  /tmp/tree-setup-repo

cp /tmp/tree-setup-repo/startup.sh /tmp/startup.sh
echo "  ✅ Startup script opgehaald"

# ============================================================
# 3. Instance template
# ============================================================
echo ""
echo "📝 Stap 3: Instance template aanmaken..."
if gcloud compute instance-templates describe "$INSTANCE_TEMPLATE" --project="$PROJECT_ID" &>/dev/null; then
  echo "  ⏭️  $INSTANCE_TEMPLATE bestaat al, overgeslagen"
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
echo "🏭 Stap 4: Managed Instance Group aanmaken..."
if gcloud compute instance-groups managed describe "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" &>/dev/null; then
  echo "  ⏭️  $MIG_NAME bestaat al"
  echo "  🔄 Template wisselen naar $INSTANCE_TEMPLATE en VM's vervangen..."
  gcloud compute instance-groups managed set-instance-template "$MIG_NAME" \
    --zone="$ZONE" \
    --project="$PROJECT_ID" \
    --template="$INSTANCE_TEMPLATE"

  gcloud compute instance-groups managed rolling-action replace "$MIG_NAME" \
    --zone="$ZONE" \
    --project="$PROJECT_ID" \
    --max-unavailable=1
  echo "  ✅ Rolling replace gestart"
else
  gcloud compute instance-groups managed create "$MIG_NAME" \
    --project="$PROJECT_ID" \
    --zone="$ZONE" \
    --template="$INSTANCE_TEMPLATE" \
    --size=1 \
    --base-instance-name=treeapp
fi

# ============================================================
# 5. Autoscaling
# ============================================================
echo ""
echo "📈 Stap 5: Autoscaling configureren..."
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
echo "🔥 Stap 6: Firewall regel voor poort 8080..."
if gcloud compute firewall-rules describe allow-http-8080 --project="$PROJECT_ID" &>/dev/null; then
  echo "  ⏭️  allow-http-8080 bestaat al, overgeslagen"
else
  gcloud compute firewall-rules create allow-http-8080 \
    --project="$PROJECT_ID" \
    --direction=INGRESS \
    --action=ALLOW \
    --rules=tcp:8080 \
    --source-ranges=0.0.0.0/0 \
    --target-tags=http-server
fi

echo ""
echo "✅ Setup voltooid voor branch: $BRANCH"
echo ""
echo "⏳ Wacht 5-8 minuten tot de VM volledig opgestart is."
echo "📋 Check status met:"
echo "   gcloud compute instance-groups managed list-instances $MIG_NAME --zone=$ZONE --project=$PROJECT_ID"
echo ""
echo "🌐 Vind het externe IP:"
echo "   gcloud compute instances list --filter=\"name~treeapp\" --format=\"value(name,EXTERNAL_IP)\""