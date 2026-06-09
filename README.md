<div align="center">

# TreeApp

**A participatory platform for gathering community ideas and feedback**

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-containerized-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![GCP](https://img.shields.io/badge/Google%20Cloud-deployed-4285F4?logo=google-cloud&logoColor=white)](https://cloud.google.com/)
[![Gemini AI](https://img.shields.io/badge/Gemini%20AI-integrated-8E75B2?logo=google&logoColor=white)](https://ai.google.dev/)

*School project — Karel de Grote Hogeschool, Integration Project Year 2 (2025–2026)*

</div>

---

## What is TreeApp?

TreeApp is a **multi-tenant participatory platform** that lets organizations create projects and collect structured community feedback. Think of it as a digital town square: organizations post projects, citizens submit ideas, vote, and take surveys — all moderated automatically by AI.

Each organization gets its own branded **subplatform** with its own projects, topics, and user base. Submitted ideas are automatically screened for toxic content using Google Gemini before they go live.

---

## Features

- **Multi-tenant subplatforms** — each organization has an isolated branded environment
- **Project management** — create projects with configurable participation types (open ideas, surveys, Q&A)
- **Idea submission** — users submit ideas with reactions, media, and topic tags
- **AI content moderation** — Gemini 2.5 Flash automatically checks submissions for toxicity
- **AI image generation** — Imagen 3 generates cover images for projects
- **Surveys & questions** — structured data collection alongside open ideas
- **Statistics dashboard** — admins see participation trends, idea counts, sentiment breakdowns
- **Role-based access** — platform admin, sub-admin, and regular user roles
- **Rate limiting** — Cloud Armor (300 req/min) + app-level AI limiter (60 AI calls/hour)
- **Zero-downtime deploys** — rolling updates via Managed Instance Group

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | ASP.NET Core MVC, Tailwind CSS, TypeScript, Vite |
| **Backend** | ASP.NET Core 9, C# |
| **Database** | PostgreSQL 16 (Cloud SQL) |
| **ORM** | Entity Framework Core |
| **AI** | Google Gemini 2.5 Flash (moderation), Imagen 3 (image generation) |
| **Auth** | ASP.NET Core Identity |
| **Storage** | Google Cloud Storage (user-uploaded images) |
| **Container** | Docker, Docker Compose |
| **Infrastructure** | Google Cloud Platform (see below) |

---

## Architecture

```
Internet
   │
   ├── :80  → HTTP Forwarding Rule → redirect to HTTPS
   │
   └── :443 → HTTPS Load Balancer (Cloud Armor: 300 req/min per IP)
                   │
              Backend Service (session affinity, 24h cookie)
                   │
         Managed Instance Group (1–3 VMs, autoscale at 60% CPU)
                   │
              VM: e2-medium
              ┌──────────────────┬─────────────────┐
              │  web:8080        │  cloudsql-proxy  │
              │  ASP.NET Core    │  :5432           │
              │  + Rate Limiter  │                  │
              └──────────────────┴────────┬─────────┘
                                          │
                             Cloud SQL PostgreSQL 16
```

**GCP services used:**

| Service | Purpose |
|---------|---------|
| Compute Engine (MIG) | Auto-scaling VM pool |
| Cloud SQL | Managed PostgreSQL database |
| Cloud Load Balancing | HTTPS termination, health checks |
| Cloud Armor | DDoS protection, rate limiting |
| Certificate Manager | Free auto-renewing SSL certificate |
| Secret Manager | Secure storage for credentials |
| Cloud Storage | User-uploaded images |
| Cloud DNS | Automated DNS management |

---

## Project Structure

```
integratieproject/
├── Domain/          # Domain models (Idea, Project, SubPlatform, Survey, …)
├── DAL/             # Data access layer (EF Core, repositories)
├── BL/              # Business logic (managers, AI integration)
├── UI-MVC/          # ASP.NET Core MVC app (controllers, views, Tailwind)
├── Tests/           # Integration tests
├── bootstrap.sh     # One-command GCP deploy from scratch
├── setup.sh         # Build/update GCP infrastructure
├── teardown.sh      # Tear down all GCP resources
├── upgrade.sh       # Zero-downtime app update on VM
├── backup.sh        # Cloud SQL on-demand backup
└── restore.sh       # Restore database from backup
```

---

## Running Locally

**Prerequisites:** .NET 9 SDK, Docker, PostgreSQL 16

```bash
# 1. Clone the repo
git clone https://github.com/AltunbayYusuf/TreeApp.git
cd TreeApp

# 2. Start the database
docker compose up -d db

# 3. Set your connection string (appsettings.Development.json)
#    DefaultConnection: Host=localhost;Port=5432;Database=TreeApp;Username=postgres;Password=...

# 4. Run the app
cd UI-MVC
dotnet run
```

The app will be available at `https://localhost:5001`.

> **Note:** AI features (Gemini moderation, Imagen) require a Google Cloud project with Vertex AI enabled and valid credentials configured in `appsettings.json`.

---

## Deployment (GCP)

Full one-command deployment from scratch:

```bash
bash bootstrap.sh <your-domain.com> <gcp-project-id>
```

This sets up the entire GCP infrastructure automatically — SSL certificate, load balancer, autoscaling VMs, Cloud SQL, secrets, and DNS. See [DEPLOYMENT.md](DEPLOYMENT.md) for the full reference.

---

## Scripts Reference

| Script | Run on | Description |
|--------|--------|-------------|
| `bootstrap.sh <domain> [project]` | Local | Full deploy from scratch |
| `setup.sh [branch] [domain]` | Local | Build or update infrastructure |
| `teardown.sh` | Local | Remove all GCP resources |
| `backup.sh` | Local | Create on-demand database backup |
| `restore.sh` | Local | Restore database from backup |
| `upgrade.sh [branch]` | VM | Zero-downtime app update |

---

## School Context

This project was built as the **Integration Project** for year 2 at [Karel de Grote Hogeschool](https://www.kdg.be/) (Antwerp, Belgium). The goal was to design, build, and deploy a full-stack web application end-to-end — from domain modeling to production cloud infrastructure.

**Team:** Echo 20

---

<div align="center">

Made with ASP.NET Core 9 · Deployed on Google Cloud Platform

</div>
