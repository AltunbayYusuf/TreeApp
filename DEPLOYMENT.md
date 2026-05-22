# Deployment documentatie — TreeApp

Dit document beschrijft hoe de cloud omgeving opgezet, beheerd en afgebroken wordt.
De applicatie draait op **Google Cloud Platform (GCP)** met de volgende stack:

- **ASP.NET Core 9** MVC applicatie (Docker container)
- **Cloud SQL** PostgreSQL 16 (beheerde database)
- **Cloud SQL Proxy** (veilige verbinding tussen app en database)
- **Managed Instance Group** met autoscaling (horizontaal schalen)
- **HTTPS Load Balancer** met Google-managed SSL certificaat (gratis)
- **Google Secret Manager** (veilig opslag van secrets)

---

## Inhoudsopgave

1. [Vereisten](#1-vereisten)
2. [Secrets aanmaken](#2-secrets-aanmaken)
3. [Omgeving opbouwen](#3-omgeving-opbouwen)
4. [HTTPS instellen](#4-https-instellen)
5. [Autoscaling](#5-autoscaling)
6. [Session state management](#6-session-state-management)
7. [Cloud Armor (rate limiting & DDoS bescherming)](#7-cloud-armor-rate-limiting--ddos-bescherming)
8. [Bescherming AI tokens](#8-bescherming-ai-tokens)
9. [Database backup en restore](#9-database-backup-en-restore)
10. [Zero-downtime upgrade](#10-zero-downtime-upgrade)
11. [Omgeving afbreken](#11-omgeving-afbreken)
12. [Scripts overzicht](#12-scripts-overzicht)

---

## 1. Vereisten

- `gcloud` CLI geïnstalleerd en ingelogd: `gcloud auth login`
- Juist project geselecteerd: `gcloud config set project integratieproject-mvp`
- Secrets bestaan al in Secret Manager (zie stap 2)
- Service account heeft de volgende rollen:
  - `Cloud SQL Client`
  - `Secret Manager Secret Accessor`
  - `Compute Instance Admin`

---

## 2. Secrets aanmaken

Alle gevoelige gegevens worden beheerd via **Google Secret Manager**.
Secrets worden nooit hardcoded in scripts of configuratiebestanden.

Secrets aanmaken (eenmalig):

```bash
# Database wachtwoord
echo -n "mijn-db-wachtwoord" | gcloud secrets create db-password \
  --data-file=- --project=integratieproject-mvp

# Gemini API sleutel
echo -n "mijn-gemini-api-key" | gcloud secrets create gemini-api-key \
  --data-file=- --project=integratieproject-mvp

# Service account sleutel voor Cloud SQL Proxy (JSON bestand)
gcloud secrets create sa-key \
  --data-file=sa-key.json --project=integratieproject-mvp

# GitLab deploy credentials
echo -n "deploy-username" | gcloud secrets create gitlab-deploy-username \
  --data-file=- --project=integratieproject-mvp

echo -n "deploy-token" | gcloud secrets create gitlab-deploy-token \
  --data-file=- --project=integratieproject-mvp
```

Secret updaten (nieuwe versie toevoegen):

```bash
echo -n "nieuw-wachtwoord" | gcloud secrets versions add db-password \
  --data-file=- --project=integratieproject-mvp
```

Secret bekijken:

```bash
gcloud secrets versions access latest --secret=db-password --project=integratieproject-mvp
```

---

## 3. Omgeving opbouwen

Het volledige opbouwproces wordt gedaan met één script:

```bash
bash setup.sh [BRANCH] [DOMAIN]
```

**Voorbeelden:**

```bash
# Alleen HTTP (directe VM toegang op poort 8080)
bash setup.sh main

# Met HTTPS load balancer
bash setup.sh main treeapp.example.com

# Feature branch met HTTPS
bash setup.sh feature/nieuwe-feature staging.example.com
```

**Wat `setup.sh` doet (stap voor stap):**

| Stap | Wat |
|------|-----|
| 1 | Cloud SQL Postgres instantie aanmaken (als niet bestaat) |
| 2 | Startup script kopiëren naar `/tmp` |
| 3 | Instance template aanmaken met startup script en branch naam |
| 4 | Managed Instance Group (MIG) aanmaken of updaten |
| 5 | Autoscaling configureren (1–3 VMs, 60% CPU target) |
| 6 | Firewall regel voor poort 8080 |
| 7 | Statisch IP adres reserveren |
| 8 | HTTP health check + named ports op de MIG |
| 9 | Backend service met **session affinity** (GENERATED_COOKIE, 24u) |
| 10 | HTTPS load balancer + Google-managed SSL + HTTP→HTTPS redirect *(alleen als DOMAIN opgegeven)* |

**Na het uitvoeren:**

```bash
# VM status controleren
gcloud compute instance-groups managed list-instances echo20-mig \
  --zone=europe-west1-b --project=integratieproject-mvp

# Extern IP van de VM vinden
gcloud compute instances list \
  --filter="name~echo20" \
  --format="value(name,EXTERNAL_IP)"

# Statisch LB IP bekijken
gcloud compute addresses describe echo20-ip \
  --global --project=integratieproject-mvp --format="value(address)"
```

---

## 4. HTTPS instellen

HTTPS wordt geregeld via een **Google Cloud HTTPS Load Balancer** met een **Google-managed SSL certificaat**.
Dit certificaat is **gratis** en wordt automatisch vernieuwd door Google.

### Wildcard certificaten

Google-managed SSL certificaten ondersteunen wildcard domeinen (`*.example.com`):

```bash
bash setup.sh main "*.example.com"
# of meerdere domeinen:
bash setup.sh main "treeapp.example.com,www.treeapp.example.com"
```

### DNS instelling (vereist)

Na het uitvoeren van `setup.sh` moet je DNS configureren:

```
DNS A-record: jouw-domein.com → <STATIC_IP_ADDRESS>
```

Het statische IP vind je met:
```bash
gcloud compute addresses describe echo20-ip --global \
  --project=integratieproject-mvp --format="value(address)"
```

### SSL provisioning

Na het instellen van de DNS duurt het **15–60 minuten** voordat Google het certificaat provisioneert.
Status controleren:

```bash
gcloud certificate-manager certificates list --project=integratieproject-mvp
```

Status `ACTIVE` = certificaat werkt.

### HTTP → HTTPS redirect

Alle HTTP-verzoeken (poort 80) worden automatisch doorgestuurd naar HTTPS (poort 443).
Dit is geconfigureerd als een aparte URL map met redirect actie in `setup.sh`.

### Architectuur HTTPS stack

```
Internet
   │
   ├── :80  → HTTP Forwarding Rule → HTTP URL Map (redirect naar HTTPS)
   │
   └── :443 → HTTPS Forwarding Rule → HTTPS Target Proxy (SSL termination)
                                            │
                                       URL Map → Backend Service
                                                       │
                                               MIG (1–3 VMs)
                                                       │
                                              Docker: web:8080
                                                       │
                                            Cloud SQL Proxy:5432
                                                       │
                                               Cloud SQL PostgreSQL
```

---

## 5. Autoscaling

De applicatie schaalt **horizontaal** via een **Managed Instance Group (MIG)**.

**Configuratie:**

| Parameter | Waarde |
|-----------|--------|
| Min VMs | 1 |
| Max VMs | 3 |
| CPU trigger | 60% gemiddeld |
| Cooldown | 600 seconden (10 min) |
| Machine type | e2-medium (2 vCPU, 4 GB RAM) |

Bij hoge CPU-belasting (> 60% gemiddeld over alle VMs) voegt GCP automatisch een extra VM toe.
Bij lage belasting wordt er ingekrompen tot minimaal 1 VM.

**Status bekijken:**

```bash
gcloud compute instance-groups managed list-instances echo20-mig \
  --zone=europe-west1-b --project=integratieproject-mvp

gcloud compute instance-groups managed describe echo20-mig \
  --zone=europe-west1-b --project=integratieproject-mvp
```

**Handmatig schalen (tijdelijk):**

```bash
gcloud compute instance-groups managed resize echo20-mig \
  --size=2 --zone=europe-west1-b --project=integratieproject-mvp
```

---

## 6. Session state management

Sessies worden **server-side** opgeslagen (ASP.NET Core `AddSession()`).
Bij horizontaal schalen (meerdere VMs) zijn twee mechanismen actief om sessies correct te laten werken:

### Session affinity (sticky sessions)

De load balancer gebruikt **GENERATED_COOKIE affinity**: elke gebruiker krijgt een cookie die hem/haar altijd naar dezelfde VM stuurt. TTL: 24 uur.

Dit is geconfigureerd in `setup.sh` stap 9:
```bash
--session-affinity=GENERATED_COOKIE
--affinity-cookie-ttl=86400
```

### DataProtection keys in PostgreSQL

ASP.NET Core DataProtection sleutels worden opgeslagen in de **gedeelde PostgreSQL database** (`DataProtectionKeys` tabel). Dit zorgt ervoor dat alle VMs auth cookies en antiforgery tokens kunnen ontcijferen, ook als een gebruiker van VM wisselt.

Geconfigureerd in `Program.cs`:
```csharp
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<TreeDbContext>()
    .SetApplicationName("IntegratieProject");
```

---

## 7. Cloud Armor (rate limiting & DDoS bescherming)

De applicatie is beveiligd met **Google Cloud Armor**, gekoppeld aan de load balancer backend service.

**Wat het doet:**

- Throttelt IP-adressen die meer dan **60 requests per minuut** sturen (HTTP 429)
- Blokkeert automatisch volumetrische DDoS-aanvallen
- Werkt op netwerkniveau, vóór de applicatie bereikt wordt

**Configuratie:**

| Parameter | Waarde |
|-----------|--------|
| Policy | `echo20-security-policy` |
| Drempel | 60 requests/min per IP |
| Bij overschrijding | HTTP 429 (Too Many Requests) |
| Gekoppeld aan | `echo20-backend` (backend service) |

**Status bekijken:**

```bash
gcloud compute security-policies describe echo20-security-policy \
  --project=integratieproject-mvp

gcloud compute security-policies rules list echo20-security-policy \
  --project=integratieproject-mvp
```

> Cloud Armor werkt samen met de applicatie-level rate limiter (max 20 AI-verzoeken/uur per gebruiker). Cloud Armor vangt botverkeer en DDoS op netwerkniveau af; de applicatielaag beperkt legitieme gebruikers bij overconsumptie van Gemini-tokens.

---

## 8. Bescherming AI tokens

Om overconsumptie van de **Gemini API** te voorkomen is er **rate limiting** ingebouwd.

**Limiet:** maximaal **20 AI-verzoeken per uur per gebruiker**.

De identificatie van een gebruiker gebeurt via:
1. `UserIdentifier` cookie (primair)
2. IP-adres (fallback)

Bij overschrijding krijgt de gebruiker een HTTP `429 Too Many Requests` response:
```json
{
  "ok": false,
  "message": "Je hebt het maximum aantal AI-verzoeken bereikt. Probeer over een uur opnieuw."
}
```

**Rate limiting is van toepassing op:**
- `POST /api/Ideas` — idee indienen met AI toxiciteitscheck

Geconfigureerd via ASP.NET Core's ingebouwde `RateLimiter` middleware (`Program.cs`) en het `[EnableRateLimiting("ai-limit")]` attribuut op de controller action.

---

## 9. Database backup en restore

### Backup aanmaken

```bash
bash backup.sh
```

Dit maakt een **on-demand backup** van de Cloud SQL instantie `echo20-db` en toont de laatste 5 backups.

### Backup lijst bekijken

```bash
gcloud sql backups list --instance=echo20-db --project=integratieproject-mvp
```

### Restore uitvoeren

```bash
bash restore.sh
```

Het script:
1. Toont alle beschikbare backups
2. Vraagt om het backup ID in te voeren
3. Vraagt om bevestiging (`yes`)
4. Voert de restore uit

> **Let op:** Een restore overschrijft de huidige data. Maak eerst een nieuwe backup als je de huidige data wil bewaren.

### Automatische backups

Cloud SQL maakt standaard **automatische dagelijkse backups**. Dit kan beheerd worden via:

```bash
gcloud sql instances patch echo20-db \
  --backup-start-time=02:00 \
  --project=integratieproject-mvp
```

---

## 10. Zero-downtime upgrade

Om de applicatie te updaten **zonder downtime** gebruik je:

```bash
# SSH in een VM
gcloud compute ssh <vm-naam> --zone=europe-west1-b --project=integratieproject-mvp

# Upgrade uitvoeren (op de VM)
bash /opt/intergratieproject/upgrade.sh [BRANCH]
```

**Wat `upgrade.sh` doet:**

1. Secrets ophalen uit Secret Manager
2. Laatste code pullen van GitLab (opgegeven branch)
3. Nieuwe Docker image bouwen
4. Alleen de `web` container vervangen (`docker-compose up -d --no-deps web`)
   — de `cloudsql-proxy` container blijft actief, geen databaseverbinding verbroken
5. Oude images opruimen

**Rolling update via MIG** (alle VMs tegelijk updaten):

```bash
bash setup.sh <branch>
```

Dit wisselt het instance template en start een rolling replace met `--max-unavailable=1`,
zodat altijd minstens één VM beschikbaar blijft.

---

## 11. Omgeving afbreken

```bash
bash teardown.sh
```

Het script vraagt om bevestiging (`yes`) en verwijdert daarna:

- HTTPS/HTTP forwarding rules
- HTTPS/HTTP target proxies
- SSL certificaat
- URL maps
- Cloud Armor security policy
- Backend service
- Health check
- Managed Instance Group
- Alle instance templates (alle branches)
- Statisch IP adres
- Cloud SQL instantie

> **Let op:** Dit verwijdert ook alle data in de database. Maak eerst een backup als je data wil bewaren.

---

## 12. Scripts overzicht

| Script | Gebruik | Beschrijving |
|--------|---------|--------------|
| `setup.sh [BRANCH] [DOMAIN]` | Lokaal | Volledige cloud omgeving opbouwen |
| `teardown.sh` | Lokaal | Alle cloud resources verwijderen |
| `backup.sh` | Lokaal | On-demand database backup aanmaken |
| `restore.sh` | Lokaal | Database herstellen vanuit backup |
| `upgrade.sh [BRANCH]` | Op VM | App updaten zonder downtime |
| `deploy.sh` | Op VM | App starten (secrets ophalen + docker-compose) |
| `startup.sh` | Op VM (automatisch) | Uitgevoerd bij boot van elke nieuwe VM |

---

## Architectuuroverzicht

```
┌─────────────────────────────────────────────────────┐
│                  Google Cloud Platform               │
│                                                     │
│  ┌──────────────────────────────────────────────┐   │
│  │           HTTPS Load Balancer                │   │
│  │  - Google-managed SSL (gratis, auto-renew)   │   │
│  │  - HTTP → HTTPS redirect                     │   │
│  │  - Session affinity (GENERATED_COOKIE)       │   │
│  └──────────────┬───────────────────────────────┘   │
│                 │                                   │
│  ┌──────────────▼───────────────────────────────┐   │
│  │     Managed Instance Group (1–3 VMs)         │   │
│  │                                              │   │
│  │  ┌─────────────────────────────────────┐     │   │
│  │  │  VM (e2-medium)                     │     │   │
│  │  │  ┌─────────────┐ ┌───────────────┐  │     │   │
│  │  │  │ web:8080    │ │ cloudsql-proxy│  │     │   │
│  │  │  │ ASP.NET Core│ │ :5432         │  │     │   │
│  │  │  │ + Rate Limit│ │               │  │     │   │
│  │  │  └─────────────┘ └───────┬───────┘  │     │   │
│  │  └─────────────────────────┼───────────┘     │   │
│  └────────────────────────────┼─────────────────┘   │
│                               │                     │
│  ┌────────────────────────────▼─────────────────┐   │
│  │     Cloud SQL PostgreSQL 16                  │   │
│  │  - Database: TreeApp                         │   │
│  │  - DataProtectionKeys tabel (gedeeld)        │   │
│  │  - Automatische backups                      │   │
│  └──────────────────────────────────────────────┘   │
│                                                     │
│  ┌──────────────────────────────────────────────┐   │
│  │     Google Secret Manager                   │   │
│  │  db-password · gemini-api-key · sa-key      │   │
│  │  gitlab-deploy-username · gitlab-deploy-token│   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```
