# Development Environment

## Overview
Local development setup for FinTrack using .NET 10, React 19, PostgreSQL 18, and .NET Aspire for orchestration.

## Prerequisites

| Tool | Version | Installation |
|------|---------|--------------|
| .NET SDK | 10.x | https://dot.net |
| Node.js | 22.x LTS | https://nodejs.org |
| pnpm | 9.x | `npm install -g pnpm` |
| Docker | Latest | https://docker.com |
| PostgreSQL | 18.x | Via Docker or Aspire |

## Quick Start (Aspire - Recommended)

```bash
# Start everything with Aspire (PostgreSQL + API + Dashboard)
dotnet run --project src/FinTrack.AppHost

# Aspire Dashboard: https://localhost:17225
# API: https://localhost:5001
```

Aspire automatically:
- Starts PostgreSQL in a container
- Configures connection strings
- Provides OpenTelemetry traces, logs, metrics
- Shows health status in dashboard

## Quick Start (Manual)

```bash
# 1. Start database
docker compose up -d postgres

# 2. Apply migrations
dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host

# 3. Start backend (terminal 1)
dotnet run --project src/FinTrack.Host

# 4. Start frontend (terminal 2)
cd src/FinTrack.Host/ClientApp && pnpm dev
```

## Build System (Nuke)

```bash
# Build all projects
./build.sh compile

# Build and run tests
./build.sh test

# Build, test, and publish to artifacts/
./build.sh publish

# Clean build artifacts
./build.sh clean
```

## URLs

| Service | URL | Notes |
|---------|-----|-------|
| Aspire Dashboard | https://localhost:17225 | Traces, logs, metrics |
| React Dev Server | http://localhost:5173 | Vite HMR |
| ASP.NET API | https://localhost:5001 | Backend API (with Aspire) |
| ASP.NET API | http://localhost:5000 | Backend API (standalone) |
| Swagger/Scalar | /scalar/v1 | API docs |
| PostgreSQL | localhost:5432 | Database |

## Environment Setup

Create `src/FinTrack.Host/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=fintrack_dev;Username=fintrack;Password=secret"
  },
  "Llm": {
    "ApiKey": "sk-or-v1-your-key",
    "Model": "openai/gpt-4-turbo",
    "BaseUrl": "https://openrouter.ai/api/v1"
  }
}
```

## Docker Compose (Development)

```yaml
services:
  postgres:
    image: postgres:18
    environment:
      POSTGRES_USER: fintrack
      POSTGRES_PASSWORD: secret
      POSTGRES_DB: fintrack_dev
    ports:
      - "5432:5432"
    volumes:
      - fintrack_pgdata:/var/lib/postgresql/data

volumes:
  fintrack_pgdata:
```

## Common Tasks

### Add EF Migration
```bash
dotnet ef migrations add <MigrationName> \
  -p src/FinTrack.Infrastructure \
  -s src/FinTrack.Host
```

### Reset Database
```bash
docker compose down -v
docker compose up -d postgres
dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host
```

### Run Tests
```bash
# All tests (via Nuke)
./build.sh test

# All tests (direct)
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd src/FinTrack.Host/ClientApp && pnpm test
```

## Debugging

### Backend (Rider/VS Code)
- Launch profile: `FinTrack.Host`
- Environment: `Development`

### Frontend (VS Code)
- Use Chrome DevTools or React DevTools extension
- Vite provides source maps automatically

## Troubleshooting

### Port already in use
```bash
# Find process
lsof -i :5000
# Kill it
kill -9 <PID>
```

### Database connection failed
```bash
# Check if PostgreSQL is running
docker compose ps
# Check logs
docker compose logs postgres
```

### Node modules issues
```bash
cd src/FinTrack.Host/ClientApp
rm -rf node_modules pnpm-lock.yaml
pnpm install
```
