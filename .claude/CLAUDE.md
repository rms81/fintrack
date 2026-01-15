# FinTrack - Expense Tracking System

**Project:** Self-hosted expense tracking for individuals and sole proprietors
**Status:** Development (Phase 1 - Foundation)
**Primary Goal:** Build a complete expense management system with multi-profile support, intelligent CSV import, rule-based categorization, and natural language querying

---

## Quick Reference

| What | Where | Command |
|------|-------|---------|
| Start Backend | `src/FinTrack.Host` | `dotnet run --project src/FinTrack.Host` |
| Start Frontend | `src/FinTrack.Host/ClientApp` | `pnpm dev` |
| Test All | root | `dotnet test` |
| Test Frontend | `src/FinTrack.Host/ClientApp` | `pnpm test` |
| E2E Tests | `src/FinTrack.Host/ClientApp` | `pnpm e2e` |
| E2E Tests (UI) | `src/FinTrack.Host/ClientApp` | `pnpm e2e:ui` |
| Add Migration | root | `dotnet ef migrations add <Name> -p src/FinTrack.Infrastructure -s src/FinTrack.Host` |
| Update DB | root | `dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host` |
| Docker | root | `docker compose up -d` |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Browser                               │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    FinTrack.Host (BFF)                       │
│  ┌─────────────────┐  ┌─────────────────────────────────┐   │
│  │   React SPA     │  │      ASP.NET Minimal APIs       │   │
│  │   (Vite dev)    │  │      (Wolverine Handlers)       │   │
│  └─────────────────┘  └─────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│  PostgreSQL   │   │  Rules Engine │   │   OpenRouter  │
│    (EF Core)  │   │    (TOML)     │   │   (LLM API)   │
└───────────────┘   └───────────────┘   └───────────────┘
```

**Core Components:**
- **FinTrack.Host**: ASP.NET host serving React SPA + Minimal API endpoints
- **FinTrack.Core**: Domain entities, value objects, and Wolverine handlers (vertical slices)
- **FinTrack.Infrastructure**: EF Core DbContext, external service integrations (LLM, Rules Engine)

---

## Context Documentation Structure

This project uses **fractal documentation** - information organized by attention level:

### Systems (`systems/`)
Hardware, deployment, infrastructure - changes slowly
- `systems/production.md` - Production Docker deployment
- `systems/development.md` - Local development setup
- `systems/database.md` - PostgreSQL configuration

### Modules (`modules/`)
Core code systems - changes frequently
- `modules/profiles.md` - Profile management (Personal/Business)
- `modules/accounts.md` - Bank account configuration
- `modules/transactions.md` - Transaction storage and queries
- `modules/import.md` - CSV import with LLM format detection
- `modules/rules-engine.md` - TOML-based categorization rules
- `modules/dashboard.md` - Spending visualizations
- `modules/nlq.md` - Natural language query translation

### Integrations (`integrations/`)
Cross-system communication
- `integrations/openrouter.md` - LLM API for format detection and NLQ
- `integrations/postgres.md` - Database interactions via EF Core

---

## Getting Started (for Claude)

**When you start a session:**
1. Check `systems/` for deployment context
2. Check `modules/` for the feature you're working on
3. Use `integrations/` when touching LLM or database specifics
4. Check `prompts/` for phase-specific development guidance

**The context router will automatically:**
- Keep recently mentioned files HOT (full content)
- Keep related files WARM (headers only)
- Evict unmentioned files as COLD

---

## Tech Stack

### Backend
- **.NET 10** (LTS) with C# 14
- **ASP.NET Core 10** with Minimal APIs
- **Wolverine FX** for CQRS (commands/queries)
- **Entity Framework Core 10** (Code-First)
- **PostgreSQL 18** with native UUIDv7 support
- **Tomlyn** for TOML parsing (rules engine)

### Frontend
- **React 19** with TypeScript
- **Tailwind CSS 4** (CSS-first configuration)
- **Vite** as build tool
- **Recharts** for data visualization
- **TanStack Query** for server state
- **React Router 7** for routing
- **shadcn/ui** components

### Testing
- **xUnit** for .NET tests
- **Vitest** + **Testing Library** for React unit tests
- **Playwright** for E2E browser tests
- **WebApplicationFactory** for API integration tests
- **Testcontainers** for real PostgreSQL in tests

---

## Project Structure

```
src/
├── FinTrack.Host/                    # ASP.NET host + React SPA
│   ├── Program.cs                    # Minimal API endpoints
│   ├── Endpoints/                    # Endpoint definitions by feature
│   ├── ClientApp/                    # React application
│   │   ├── src/
│   │   │   ├── features/             # Feature-based organization
│   │   │   ├── components/           # Shared UI components
│   │   │   ├── hooks/                # Custom React hooks
│   │   │   └── lib/                  # Utilities, API client
│   │   ├── e2e/                      # Playwright E2E tests
│   │   │   ├── fixtures/             # Test fixtures and utilities
│   │   │   ├── auth.setup.ts         # Authentication setup
│   │   │   └── *.spec.ts             # Test files
│   │   ├── playwright.config.ts      # Playwright configuration
│   │   └── package.json
│   └── FinTrack.Host.csproj
│
├── FinTrack.Core/                    # Domain + Application logic
│   ├── Domain/                       # Entities, Value Objects, Events
│   └── Features/                     # Vertical slices (handlers)
│
├── FinTrack.Infrastructure/          # EF Core, External services
│   ├── Persistence/                  # DbContext, Configurations
│   └── Services/                     # RulesEngine, LLM, Import
│
└── tests/
    ├── FinTrack.Tests.Integration/   # API endpoint tests
    └── FinTrack.Tests.Unit/          # Logic unit tests

docs/
├── SPEC.md                           # Full technical specification
└── ARCHITECTURE.md                   # Architecture decisions

prompts/                              # Phase-specific development prompts
.claude/commands/                     # Custom Claude Code commands
```

---

## Development Workflow

**Daily:**
```bash
# Start development
docker compose up -d postgres        # Start database
dotnet run --project src/FinTrack.Host &
cd src/FinTrack.Host/ClientApp && pnpm dev
```

**Before committing:**
```bash
dotnet test
cd src/FinTrack.Host/ClientApp && pnpm test
```

**Deploy:**
```bash
docker compose build
docker compose up -d
```

---

## Critical Files

| File | Purpose | Notes |
|------|---------|-------|
| `src/FinTrack.Host/Program.cs` | App bootstrap, endpoint registration | Entry point |
| `src/FinTrack.Infrastructure/Persistence/FinTrackDbContext.cs` | EF Core DbContext | All entity configs |
| `src/FinTrack.Core/Domain/Entities/` | Domain entities | Profile, Account, Transaction, Category |
| `src/FinTrack.Infrastructure/Services/RulesEngine.cs` | TOML rules processing | Categorization logic |
| `src/FinTrack.Host/ClientApp/src/lib/api.ts` | API client | All backend calls |

---

## Environment Variables

```bash
# Required - Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=fintrack;Username=fintrack;Password=secret

# Required - LLM (OpenRouter)
Llm__ApiKey=sk-or-v1-xxx
Llm__Model=openai/gpt-4-turbo
Llm__BaseUrl=https://openrouter.ai/api/v1

# Optional - Development
ASPNETCORE_ENVIRONMENT=Development
```

---

## Dependencies on External Services

| Service | Purpose | Failure Impact | Health Check |
|---------|---------|----------------|--------------|
| PostgreSQL | Data persistence | App non-functional | `pg_isready -h localhost` |
| OpenRouter | LLM for CSV detection, NLQ | CSV format detection fails, NLQ disabled | `curl https://openrouter.ai/api/v1/models` |

---

## Coding Standards

### C# Style
- **Functional approach**: records, pattern matching, LINQ
- **File-scoped namespaces**
- **Primary constructors** for simple classes
- **Expression-bodied members** for simple methods
- **Nullable reference types** enabled

### Naming Conventions
- Commands: `CreateProfile`, `ImportTransactions`
- Queries: `GetProfiles`, `GetTransactionsByFilter`
- Handlers: Same name as command/query (Wolverine convention)
- DTOs: `ProfileDto`, `TransactionDto`

### React/TypeScript Style
- **Functional components** only
- **Custom hooks** for reusable logic
- **Colocation**: Keep related code together
- **Zod** for runtime validation

### Database Conventions
- Table names: **snake_case**, plural (`transactions`, `profiles`)
- Column names: **snake_case** (`created_at`, `profile_id`)
- Primary keys: **UUIDv7** via `uuidv7()` PostgreSQL function
- Arrays: PostgreSQL native arrays for tags (`text[]`)
- JSON: `jsonb` for flexible data

### API Conventions
- **REST** with resource-oriented URLs
- **JSON** request/response bodies
- **Problem Details** for errors (RFC 7807)
- **Pagination**: `?page=1&pageSize=20`

### Git Conventions
- **Conventional Commits**: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`
- **Feature branches**: `feature/us-1.1-user-registration`

---

## Development Phases

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | Foundation - Auth, Profiles, Accounts | Ready to start |
| 2 | Import & Rules - CSV, LLM detection, TOML engine | Planned |
| 3 | Dashboard - Transaction views, visualizations | Planned |
| 4 | NLQ & Export - Natural language queries, data portability | Planned |
| 5 | Polish - Error handling, performance, docs | Planned |

See `prompts/` directory for detailed phase prompts.

---

## Recent Changes

**[Playwright E2E Tests]:**
- Added Playwright for end-to-end browser testing
- Configured Playwright MCP server for Claude Code integration
- Created initial E2E test structure (auth, home, profiles)

**[Initial Setup]:**
- Created project structure and CLAUDE.md
- Defined architecture with BFF pattern
- Selected tech stack (.NET 10, React 19, PostgreSQL 18)
- Simplified auth from separate AuthServer to cookie-based ASP.NET Identity

---

## For New Developers

**This file helps Claude Code:**
1. Understand the FinTrack project structure
2. Avoid hallucinating non-existent integrations
3. Maintain context across long sessions
4. Coordinate across multiple concurrent instances

**Key documentation:**
- `docs/SPEC.md` - Full technical specification
- `docs/ARCHITECTURE.md` - Architecture decisions
- `prompts/` - Phase-specific development prompts
- `.claude/commands/` - Custom Claude Code commands

---

## MCP Servers

This project has a Playwright MCP server configured (`.claude/mcp_servers.json`) for browser automation:

**Capabilities:**
- Navigate to URLs and interact with pages
- Take screenshots for visual debugging
- Fill forms and click elements
- Wait for elements and network requests

**Usage in Claude Code:**
- Run E2E tests with visual feedback
- Debug UI issues by capturing screenshots
- Verify UI changes before committing

---

## Multi-Instance Coordination

If running multiple Claude Code instances on this project:

1. **Set instance ID:**
   ```bash
   export CLAUDE_INSTANCE=A  # Or B, C, D, etc.
   ```

2. **Signal when completing work:**
   ```pool
   INSTANCE: A
   ACTION: completed
   TOPIC: Profile CRUD implementation
   SUMMARY: Added CreateProfile, GetProfiles handlers with EF Core
   AFFECTS: src/FinTrack.Core/Features/Profiles/, src/FinTrack.Infrastructure/Persistence/
   BLOCKS: Account management (depends on profiles)
   ```

3. **Other instances will see your updates** at their next session start

---

**Last Updated:** 2026-01-15
**Maintained By:** Ricardo
