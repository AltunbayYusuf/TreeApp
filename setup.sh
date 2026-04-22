#!/bin/bash
# ============================================================
# setup.sh
# Bouwt de volledige cloud omgeving op vanaf nul
# Gebruik: bash setup.sh
#
# Vereist: je bent ingelogd met gcloud en hebt rechten op project
# ============================================================

set -euo pipefail

PROJECT_ID="integratieproject-mvp"
REGION="europe-west1"
ZONE="europe-west1-b"

# Cloud SQL
DB_INSTANCE="treeapp-db-new"
DB_TIER="db-f1-micro"
DB_VERSION="POSTGRES_16"
DB_NAME="TreeApp"

# MIG
INSTANCE_TEMPLATE="treeapp-template-v2"
MIG_NAME="treeapp-mig"
MACHINE_TYPE="e2-standard-2"
MIN_VMS=1
MAX_VMS=3
CPU_TARGET=0.6
COOLDOWN=60

echo "🚀 Setup gestart voor project: $PROJECT_ID"

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
    --root-password="$(gcloud secrets versions access latest --secret=db-password --project=$PROJECT_ID)" \
    --authorized-networks="0.0.0.0/0"
  
  echo "  📋 Database $DB_NAME aanmaken..."
  gcloud sql databases create "$DB_NAME" --instance="$DB_INSTANCE" --project="$PROJECT_ID"
fi

# ============================================================
# 2. Startup script downloaden
# ============================================================
echo ""
echo "📥 Stap 2: Startup script downloaden uit GitLab..."
GIT_USER=$(gcloud secrets versions access latest --secret=gitlab-deploy-username --project="$PROJECT_ID")
GIT_TOKEN=$(gcloud secrets versions access latest --secret=gitlab-deploy-token --project="$PROJECT_ID")

rm -rf /tmp/tree-setup-repo
git clone --depth 1 --branch feature/infra-scripts \
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
    --metadata-from-file=startup-script=/tmp/startup.sh
fi

# ============================================================
# 4. MIG (Managed Instance Group)
# ============================================================
echo ""
echo "🏭 Stap 4: Managed Instance Group aanmaken..."
if gcloud compute instance-groups managed describe "$MIG_NAME" --zone="$ZONE" --project="$PROJECT_ID" &>/dev/null; then
  echo "  ⏭️  $MIG_NAME bestaat al, overgeslagen"
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
echo "✅ Setup voltooid!"
echo ""
echo "⏳ Wacht 3-5 minuten tot de eerste VM volledig opgestart is."
echo "📋 Check status met:"
echo "   gcloud compute instance-groups managed list-instances $MIG_NAME --zone=$ZONE --project=$PROJECT_ID"
