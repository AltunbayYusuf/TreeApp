#!/bin/bash
# Startup script voor nieuwe VM's in de Managed Instance Group

apt-get update -y
apt-get install -y docker.io docker-compose git curl python3

systemctl enable docker
systemctl start docker

git clone https://gitlab.com/kdg-ti/integratieproject-1/2526/20_echo/intergratieproject.git /opt/intergratieproject
cd /opt/intergratieproject

bash /opt/intergratieproject/deploy.sh
