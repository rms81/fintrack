# FinTrack - Expense Tracking System

## Project Overview

FinTrack is a self-hosted expense tracking system for individuals and sole proprietors. It allows users to:
- Manage multiple profiles (Personal/Business) with logical separation
- Import bank statements via CSV with LLM-assisted format detection
- Categorize transactions using a deterministic TOML-based rules engine
- View spending through interactive dashboards
- Query data using natural language (translated to SQL)
- Export/import data for portability

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
- **shadcn/ui** components (Tailwind v4 compatible)

### Infrastructure
- **Docker** and **Docker Compose**
- **OpenRouter API** for LLM integration (OpenAI-compatible)

### Testing
- **xUnit** for .NET tests
- **Vitest** + **Testing Library** for React tests
- **WebApplicationFactory** for integration tests

## Architecture Principles

### Vertical Slicing
Organize code by feature, not by technical layer. Each feature contains its own:
- Command/Query handlers (Wolverine)
- DTOs
- Validation
- Database queries

### Lean DDD
Apply Domain-Driven Design pragmatically:
- Rich domain entities where behavior is complex
- Simple DTOs for CRUD operations
- Avoid unnecessary abstractions

### BFF Pattern
The ASP.NET backend serves as a Backend-for-Frontend:
- React SPA is served by ASP.NET
- API endpoints are co-located
- Type-safe communication

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
```

## Coding Standards

### C# Style
- **Functional approach** where practical (records, pattern matching, LINQ)
- **File-scoped namespaces**
- **Primary constructors** for simple classes
- **Required members** over constructor parameters when appropriate
- **Expression-bodied members** for simple methods
- **Nullable reference types** enabled

### Naming Conventions
- Commands: `CreateProfile`, `ImportTransactions`
- Queries: `GetProfiles`, `GetTransactionsByFilter`
- Handlers: Same name as command/query (Wolverine convention)
- DTOs: `ProfileDto`, `TransactionDto`
- Endpoints: RESTful naming

### React/TypeScript Style
- **Functional components** only
- **Custom hooks** for reusable logic
- **Colocation**: Keep related code together
- **Barrel exports** for features
- **Zod** for runtime validation

## Key Dependencies

### Backend NuGet Packages
```xml
<PackageReference Include="WolverineFx" Version="3.*" />
<PackageReference Include="WolverineFx.Http" Version="3.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<PackageReference Include="Tomlyn" Version="0.*" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*" />
```

### Frontend npm Packages
```json
{
  "react": "^19.0.0",
  "react-dom": "^19.0.0",
  "tailwindcss": "^4.0.0",
  "@tailwindcss/vite": "^4.0.0",
  "vite": "^6.0.0",
  "@tanstack/react-query": "^5.0.0",
  "react-router": "^7.0.0",
  "recharts": "^2.0.0",
  "zod": "^3.0.0"
}
```

## Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=fintrack;Username=fintrack;Password=secret

# LLM (OpenRouter)
Llm__ApiKey=sk-or-v1-xxx
Llm__Model=openai/gpt-4-turbo
Llm__BaseUrl=https://openrouter.ai/api/v1

# Auth Server (external)
Auth__Authority=https://auth.example.com
Auth__ClientId=fintrack
```

## Common Commands

```bash
# Run the application
dotnet run --project src/FinTrack.Host

# Run tests
dotnet test

# Add EF migration
dotnet ef migrations add MigrationName -p src/FinTrack.Infrastructure -s src/FinTrack.Host

# Update database
dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host

# Frontend dev
cd src/FinTrack.Host/ClientApp && npm run dev

# Docker
docker compose up -d
```

## Database Conventions

- Table names: **snake_case**, plural (`transactions`, `profiles`)
- Column names: **snake_case** (`created_at`, `profile_id`)
- Primary keys: **UUIDv7** via `uuidv7()` PostgreSQL function
- Timestamps: `created_at`, `updated_at` with defaults
- Soft deletes: Not used (hard delete with cascade)
- Arrays: PostgreSQL native arrays for tags (`text[]`)
- JSON: `jsonb` for flexible data (raw CSV rows)

## API Conventions

- **REST** with resource-oriented URLs
- **JSON** request/response bodies
- **Problem Details** for errors (RFC 7807)
- **Pagination**: `?page=1&pageSize=20`
- **Filtering**: Query parameters (`?categoryId=xxx&fromDate=2024-01-01`)
- **Sorting**: `?sort=date:desc,amount:asc`

## Testing Conventions

### Integration Tests
- Test full request/response cycle
- Use real PostgreSQL (Testcontainers or test database)
- Mock external services (LLM)
- One test class per feature area

### Unit Tests
- Test business logic in isolation
- Focus on rules engine, parsers, calculations
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`

## Git Conventions

- **Conventional Commits**: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`
- **Feature branches**: `feature/us-1.1-user-registration`
- **Small, focused commits**

## Important Files

- `CLAUDE.md` - This file (project context)
- `docs/SPEC.md` - Full technical specification
- `docs/ARCHITECTURE.md` - Architecture decisions
- `prompts/` - Phase-specific prompts for development
- `.claude/commands/` - Custom Claude Code commands

## Current Phase

The project is organized into 5 phases:
1. **Foundation** - Project setup, auth, profiles, accounts
2. **Import & Rules** - CSV import, LLM format detection, rules engine
3. **Dashboard** - Transaction views, visualizations
4. **NLQ & Export** - Natural language queries, data export/import
5. **Polish** - Error handling, performance, documentation

See `prompts/` directory for detailed phase prompts.
