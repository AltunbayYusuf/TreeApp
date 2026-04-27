#!/bin/bash
# Startup script voor nieuwe VM's in de Managed Instance Group
# Draait automatisch bij elke VM boot (als root)
# Branch wordt uit VM metadata gelezen (key: deploy-branch)

set -e

PROJECT_ID="integratieproject-mvp"

# Log alles naar een bestand zodat we kunnen debuggen via serial console
exec > >(tee -a /var/log/startup-script.log) 2>&1
echo "=== Startup script gestart op $(date) ==="

echo "=== Packages installeren ==="
apt-get update -y
apt-get install -y docker.io docker-compose git curl python3

systemctl enable docker
systemctl start docker

echo "=== Branch ophalen uit VM metadata ==="
BRANCH=$(curl -s -f "http://metadata.google.internal/computeMetadata/v1/instance/attributes/deploy-branch" -H "Metadata-Flavor: Google" || echo "main")
echo "Branch: $BRANCH"

echo "=== GitLab credentials ophalen uit Secret Manager ==="
TOKEN_URL="http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token"
ACCESS_TOKEN=$(curl -s "$TOKEN_URL" -H "Metadata-Flavor: Google" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

GIT_USER=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/gitlab-deploy-username/versions/latest:access" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

GIT_TOKEN=$(curl -s "https://secretmanager.googleapis.com/v1/projects/$PROJECT_ID/secrets/gitlab-deploy-token/versions/latest:access" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | python3 -c "import sys,json,base64; print(base64.b64decode(json.load(sys.stdin)['payload']['data']).decode())")

echo "=== Repo klonen (branch: $BRANCH) ==="
rm -rf /opt/intergratieproject
git clone --branch "$BRANCH" "https://$GIT_USER:$GIT_TOKEN@gitlab.com/kdg-ti/integratieproject-1/2526/20_echo/intergratieproject.git" /opt/intergratieproject

echo "=== Deploy script uitvoeren ==="
cd /opt/intergratieproject
bash /opt/intergratieproject/deploy.sh

echo "=== Startup script klaar op $(date) ==="