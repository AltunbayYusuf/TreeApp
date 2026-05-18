#!/bin/bash
set -euo pipefail

PROJECT_ID="integratieproject-mvp"
INSTANCE="echo20-db"

echo " Backup aanmaken voor Cloud SQL instance: $INSTANCE"

gcloud sql backups create \
  --instance="$INSTANCE" \
  --project="$PROJECT_ID"

echo ""
echo " Overzicht van recente backups:"
gcloud sql backups list \
  --instance="$INSTANCE" \
  --project="$PROJECT_ID" \
  --limit=5

echo " Backup aangemaakt!"
