#!/bin/bash
# ============================================================
# restore.sh
# Zet een Cloud SQL backup terug
# Gebruik: bash restore.sh
# ============================================================

set -euo pipefail

PROJECT_ID="integratieproject-mvp"
INSTANCE="echo20-db"

echo " Beschikbare backups:"
gcloud sql backups list \
  --instance="$INSTANCE" \
  --project="$PROJECT_ID"

echo ""
read -rp "Voer het backup ID in dat je wil restoren: " BACKUP_ID

echo "  Dit overschrijft de huidige database. Ben je zeker? (yes/no)"
read -r CONFIRM
if [ "$CONFIRM" != "yes" ]; then
  echo "Geannuleerd."
  exit 0
fi

echo " Restoring backup $BACKUP_ID..."
gcloud sql backups restore "$BACKUP_ID" \
  --restore-instance="$INSTANCE" \
  --project="$PROJECT_ID"

echo " Restore voltooid!"
