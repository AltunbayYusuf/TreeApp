#!/bin/bash
# ============================================================
# deploy-demo.sh
# Wrapper rond bootstrap.sh met automatische retry bij volledige crashes.
# Bedoeld voor de jury-demo waar je niet kunt herstarten met de hand.
#
# Gebruik: bash deploy-demo.sh <DOMAIN> <PROJECT_ID>
# Voorbeeld: bash deploy-demo.sh kdg-hogeschool.test.echo20.com echo20-demo-test-2
#
# Bescherming in 2 lagen:
#  1. bootstrap.sh en setup.sh hebben een ingebouwde retry() rond
#     kritieke gcloud calls (5s -> 10s -> 20s backoff).
#  2. Deze wrapper herstart het hele bootstrap-script tot 3x als het
#     toch volledig crasht. bootstrap is idempotent, dus elke poging
#     pakt op waar de vorige stopte.
# ============================================================

set -uo pipefail

MAX_ATTEMPTS=3
DELAY_BETWEEN_ATTEMPTS=30

if [ $# -lt 2 ]; then
  echo "Gebruik: bash deploy-demo.sh <DOMAIN> <PROJECT_ID>"
  echo "Voorbeeld: bash deploy-demo.sh kdg-hogeschool.test.echo20.com echo20-demo-test-2"
  exit 1
fi

DOMAIN="$1"
PROJECT_ID="$2"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

for ATTEMPT in $(seq 1 $MAX_ATTEMPTS); do
  echo ""
  echo "================================================================"
  echo " deploy-demo.sh - poging $ATTEMPT van $MAX_ATTEMPTS"
  echo " domein  : $DOMAIN"
  echo " project : $PROJECT_ID"
  echo "================================================================"

  if bash "$SCRIPT_DIR/bootstrap.sh" "$DOMAIN" "$PROJECT_ID"; then
    echo ""
    echo "================================================================"
    echo " Deployment geslaagd op poging $ATTEMPT"
    echo "================================================================"
    exit 0
  fi

  if [ $ATTEMPT -lt $MAX_ATTEMPTS ]; then
    echo ""
    echo "Bootstrap mislukt op poging $ATTEMPT. Wachten ${DELAY_BETWEEN_ATTEMPTS}s en opnieuw proberen..."
    sleep $DELAY_BETWEEN_ATTEMPTS
  fi
done

echo ""
echo "================================================================"
echo " FOUT: deployment mislukt na $MAX_ATTEMPTS pogingen"
echo "================================================================"
exit 1
