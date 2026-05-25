# TreeApp — Deployment vanaf nul

Dit document legt uit hoe je TreeApp volledig deployt op Google Cloud Platform (GCP) vanaf een nieuwe machine.

---

## Wat je nodig hebt

| Vereiste | Details |
|----------|---------|
| **Bash** | Git Bash (Windows), Terminal (Mac/Linux) |
| **gcloud CLI** | [Installeren](https://cloud.google.com/sdk/docs/install) |
| **GCP project + billing** | Eigen Google Cloud project met actief billing account |
| **DNS toegang** | Mogelijkheid om CNAME + A-record in te stellen bij je DNS provider |
| **GitLab deploy token** | GitLab > Settings > Repository > Deploy tokens |
| **Gemini API sleutel** | [https://aistudio.google.com/apikey](https://aistudio.google.com/apikey) |

---

## Quickstart

```bash
# 1. Clone de repo
git clone https://gitlab.com/kdg-ti/integratieproject-1/2526/20_echo/integratieproject.git
cd integratieproject

# 2. Draai bootstrap met jouw domein + project
bash bootstrap.sh <subdomain>.<jouw-domein.com> <jouw-gcp-project-id>

# Voorbeelden
bash bootstrap.sh kdg-hogeschool.echo20.com                       # default project
bash bootstrap.sh kdg-hogeschool.echo20.com mijn-project-id       # ander GCP project
bash bootstrap.sh kdg-hogeschool.test.echo20.com demo-project-id  # test-deployment naast prod
```

Het script leidt het base domein automatisch af door het eerste label te strippen:
- `kdg-hogeschool.echo20.com` → wildcard cert op `*.echo20.com`
- `kdg-hogeschool.test.echo20.com` → wildcard cert op `*.test.echo20.com` (handig voor een tweede deployment naast prod, zonder DNS-conflict)

Het script vraagt tijdens de uitvoering om:
- Database wachtwoord (zelf kiezen)
- Gemini API sleutel
- GitLab deploy token gebruikersnaam + waarde

Na afloop geeft het script een IP-adres. Stel dat in als A-record bij je DNS provider:
```
kdg-hogeschool.echo20.com  ->  <IP uit script output>
```

Wacht 15-60 minuten voor SSL activatie, dan is de applicatie bereikbaar op `https://kdg-hogeschool.echo20.com`.

---

## Wat bootstrap.sh doet

| Stap | Actie |
|------|-------|
| 1 | Prerequisites controleren (gcloud, curl) |
| 2 | Inloggen bij GCP via browser (`gcloud auth login`) |
| 3 | Benodigde GCP APIs inschakelen |
| 4 | Service account aanmaken met Cloud SQL + Secret Manager rechten |
| 5 | Secrets opslaan in Google Secret Manager |
| 6 | Wildcard SSL certificaat aanmaken via Certificate Manager |
| 7 | Volledige infrastructure opbouwen via `setup.sh` |

`setup.sh` bouwt de volgende GCP resources:

```
Internet
   |
   +-- :80  --> HTTP redirect naar HTTPS
   |
   +-- :443 --> HTTPS Load Balancer (Cloud Armor: 60 req/min per IP)
                    |
               Backend Service (session affinity, 24u cookie)
                    |
               Managed Instance Group (1-3 VMs, autoscale op 60% CPU)
                    |
               VM: Docker (ASP.NET Core :8080 + Cloud SQL Proxy :5432)
                    |
               Cloud SQL PostgreSQL 16
```

---

## Scripts overzicht

| Script | Waar | Gebruik |
|--------|------|---------|
| `bootstrap.sh <DOMAIN>` | Lokaal | Alles vanaf nul deployen (eerste keer) |
| `setup.sh [BRANCH] [DOMAIN]` | Lokaal | Infrastructure opbouwen of updaten |
| `teardown.sh` | Lokaal | Alle GCP resources verwijderen |
| `backup.sh` | Lokaal | Database backup aanmaken |
| `restore.sh` | Lokaal | Database herstellen vanuit backup |
| `upgrade.sh [BRANCH]` | Op VM | App updaten zonder downtime |

---

## Opnieuw deployen (bestaande omgeving)

Na de eerste bootstrap gebruik je voor updates:

```bash
# Code update op draaiende VM (zero-downtime)
gcloud compute ssh <vm-naam> --zone=europe-west1-b --project=integratieproject-mvp
bash /opt/intergratieproject/upgrade.sh main

# Of volledig opnieuw opbouwen
bash setup.sh main kdg-hogeschool.echo20.com
```

---

## Omgeving verwijderen

```bash
bash teardown.sh
```

Vraagt om bevestiging. Verwijdert alle GCP resources inclusief database.
Maak eerst een backup: `bash backup.sh`

---

## Nuttige commando's

Vervang `<PROJECT_ID>` met je eigen GCP project ID (default: `integratieproject-mvp`).

```bash
# VM status
gcloud compute instance-groups managed list-instances treeapp-mig \
  --zone=europe-west1-b --project=<PROJECT_ID>

# SSL certificaat status
gcloud certificate-manager certificates list --project=<PROJECT_ID>

# Statisch IP bekijken
gcloud compute addresses describe treeapp-ip \
  --global --project=<PROJECT_ID> --format="value(address)"

# Cloud Armor status
gcloud compute security-policies describe treeapp-security-policy \
  --project=<PROJECT_ID>
```
