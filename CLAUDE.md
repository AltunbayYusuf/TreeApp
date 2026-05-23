# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Local development
```bash
# Start local database + app (Docker)
docker-compose up

# Start only database, run app directly
docker-compose up db
cd UI-MVC && dotnet run

# Frontend (TypeScript + Tailwind via Vite)
cd UI-MVC && npm install && npm run build
npm run dev   # watch mode during development
```

### Tests
```bash
dotnet test                          # all tests
dotnet test --filter "ClassName=X"   # single class
dotnet test --filter "Name=TestMethodName"
```

### Production deployment
```bash
bash bootstrap.sh kdg-hogeschool.echo20.com   # first time, fresh machine
bash setup.sh main kdg-hogeschool.echo20.com  # rebuild infrastructure
bash upgrade.sh main                          # zero-downtime code update (run on VM)
bash teardown.sh                              # destroy all GCP resources
```

## Architecture

4-layer solution — all projects are in separate folders:

```
Domain/        → Entity models (no dependencies)
DAL/           → EF Core DbContext, migrations, repositories
BL/            → Services, AI integration, business rules
UI-MVC/        → ASP.NET Core 9 MVC, controllers, Razor views, Vite frontend
```

### Multi-tenancy (subdomain routing)
The app is multi-tenant per subdomain. A custom middleware (`SubdomainMiddleware`) extracts the tenant slug from:
1. Subdomain (e.g. `kdg-hogeschool` from `kdg-hogeschool.echo20.com`)
2. Route prefix fallback
3. Query string / cookie fallback

The slug resolves to a `Platform` entity. Routes like `/{slug}/ideas` are guarded to not collide with controller names — the slug regex requires a hyphen (`[a-z][a-z0-9-]*-[a-z0-9-]+`) to prevent controller names (Project, Idea, Platform) from being matched as slugs.

### Authentication & roles
ASP.NET Identity with two roles: `GeneralAdmin` (manages platforms) and `SubAdmin` (manages a specific platform). DataProtection keys are persisted to the PostgreSQL `DataProtectionKeys` table so all VMs in the MIG can share auth cookies.

### AI integration
- **Moderation**: Gemini 2.5 Flash via Google Vertex AI — called on idea submission (`POST /api/Ideas`) to check for toxic content
- **Image generation**: Imagen 3.0 via Vertex AI — images stored in GCP Cloud Storage buckets
- **Rate limiting**: 20 AI requests/hour per user identified by `UserIdentifier` cookie (fallback: IP). Enforced via ASP.NET Core `RateLimiter` with `[EnableRateLimiting("ai-limit")]`

### Frontend build
Vite builds TypeScript + Tailwind into `UI-MVC/wwwroot/dist`. The Docker multi-stage build runs `npm run build` before the .NET publish step. During local development use `npm run dev` for hot reload.

### Docker setup
- `docker-compose.yaml` — local dev: PostgreSQL + app
- `docker-compose.cloud.yaml` — production: app + Cloud SQL Proxy sidecar (reads `sa-key.json` from `/tmp/sa-key.json`)

### GCP infrastructure (managed by scripts)
- **Cloud SQL** PostgreSQL 16 (`echo20-db`, `db-f1-micro`)
- **MIG** autoscaling 1–3 × `e2-medium`, health check on `GET /health`
- **Cloud Armor** on backend service: 60 req/min per IP → HTTP 429
- **Certificate Manager** wildcard cert `*.echo20.com` (`treeapp-cert-map`)
- **Secret Manager** secrets: `db-password`, `gemini-api-key`, `sa-key`, `gitlab-deploy-username`, `gitlab-deploy-token`

VM startup flow: `startup.sh` (boot) → fetches GitLab credentials from Secret Manager → clones repo → runs `deploy.sh` → `docker-compose.cloud.yaml up`.
