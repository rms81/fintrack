# Production Environment

## Overview
FinTrack production deployment using Docker containers.

## Architecture

```
┌─────────────────────────────────────────┐
│              Reverse Proxy              │
│            (Traefik/Nginx)              │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│           FinTrack.Host                 │
│     (ASP.NET + React SPA)               │
│         Port: 8080                      │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│            PostgreSQL 18                │
│           Port: 5432                    │
│      Volume: fintrack_pgdata            │
└─────────────────────────────────────────┘
```

## Docker Compose (Production)

```yaml
services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=fintrack;Username=fintrack;Password=${POSTGRES_PASSWORD}
      - Llm__ApiKey=${LLM_API_KEY}
      - Llm__Model=openai/gpt-4-turbo
      - Llm__BaseUrl=https://openrouter.ai/api/v1
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped

  postgres:
    image: postgres:18
    environment:
      POSTGRES_USER: fintrack
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: fintrack
    volumes:
      - fintrack_pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U fintrack"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  fintrack_pgdata:
```

## Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/FinTrack.Host/FinTrack.Host.csproj src/FinTrack.Host/
COPY src/FinTrack.Core/FinTrack.Core.csproj src/FinTrack.Core/
COPY src/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj src/FinTrack.Infrastructure/
RUN dotnet restore src/FinTrack.Host/FinTrack.Host.csproj

# Copy everything and build
COPY . .
WORKDIR /src/src/FinTrack.Host

# Build React app
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs \
    && cd ClientApp && npm ci && npm run build

# Build .NET app
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FinTrack.Host.dll"]
```

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `POSTGRES_PASSWORD` | Yes | PostgreSQL password |
| `LLM_API_KEY` | Yes | OpenRouter API key |
| `ASPNETCORE_ENVIRONMENT` | No | Default: Production |

## Deployment Steps

### Initial Deploy
```bash
# 1. Create .env file
cat > .env << EOF
POSTGRES_PASSWORD=<strong-password>
LLM_API_KEY=sk-or-v1-xxx
EOF

# 2. Build and start
docker compose -f docker-compose.prod.yml up -d --build

# 3. Check logs
docker compose -f docker-compose.prod.yml logs -f
```

### Update Deploy
```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker compose -f docker-compose.prod.yml up -d --build

# Run migrations (if needed)
docker compose -f docker-compose.prod.yml exec app \
  dotnet ef database update
```

## Backup & Restore

### Database Backup
```bash
# Create backup
docker compose exec postgres pg_dump -U fintrack fintrack > backup_$(date +%Y%m%d).sql

# With compression
docker compose exec postgres pg_dump -U fintrack fintrack | gzip > backup_$(date +%Y%m%d).sql.gz
```

### Database Restore
```bash
# Stop app first
docker compose stop app

# Restore
cat backup.sql | docker compose exec -T postgres psql -U fintrack fintrack

# Start app
docker compose start app
```

## Health Checks

### Application
```bash
curl -f http://localhost:8080/health
```

### Database
```bash
docker compose exec postgres pg_isready -U fintrack
```

## Monitoring

### View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f app
```

### Resource Usage
```bash
docker stats
```

## Security Considerations

- [ ] Use HTTPS (configure reverse proxy)
- [ ] Set strong `POSTGRES_PASSWORD`
- [ ] Rotate `LLM_API_KEY` periodically
- [ ] Enable PostgreSQL SSL connections
- [ ] Configure firewall rules
- [ ] Regular backups with off-site storage

## Troubleshooting

### App won't start
```bash
# Check logs
docker compose logs app

# Check database connectivity
docker compose exec app dotnet ef database update --dry-run
```

### Database connection refused
```bash
# Check if postgres is running
docker compose ps postgres

# Check postgres logs
docker compose logs postgres
```

### Out of disk space
```bash
# Clean old images
docker system prune -a

# Check volume size
docker system df -v
```
