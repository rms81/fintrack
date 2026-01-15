# FinTrack - Expense Tracking System

Self-hosted expense tracking for individuals and sole proprietors with multi-profile support, intelligent CSV import, rule-based categorization, and natural language querying.

## Quick Start

### With .NET Aspire (Recommended)

```bash
# Start everything (PostgreSQL + API + Aspire Dashboard)
dotnet run --project src/FinTrack.AppHost
```

- **Aspire Dashboard**: https://localhost:17225 (traces, logs, metrics)
- **API**: https://localhost:5001

### Manual Setup

```bash
# Start PostgreSQL
docker compose up -d postgres

# Run migrations
dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host

# Start API
dotnet run --project src/FinTrack.Host

# Start frontend (separate terminal)
cd src/FinTrack.Host/ClientApp && pnpm dev
```

## Build System (Nuke)

```bash
./build.sh compile    # Build all projects
./build.sh test       # Build and run tests
./build.sh publish    # Build, test, and publish to artifacts/
```

## Claude Code Development

This project is optimized for development with Claude Code.

```bash
claude
```

Claude Code will automatically read `.claude/CLAUDE.md` for project context.

## Project Structure

```
fintrack-claude-code/
├── src/
│   ├── FinTrack.Host/             # ASP.NET host + React SPA
│   ├── FinTrack.Core/             # Domain + Application logic
│   ├── FinTrack.Infrastructure/   # EF Core, External services
│   ├── FinTrack.AppHost/          # .NET Aspire orchestrator
│   └── FinTrack.ServiceDefaults/  # Shared service defaults
├── tests/
│   ├── FinTrack.Tests.Integration/
│   └── FinTrack.Tests.Unit/
├── build/
│   ├── Build.cs                   # Nuke build targets
│   └── _build.csproj
├── .claude/
│   ├── CLAUDE.md                  # Main project context
│   ├── commands/                  # Custom slash commands
│   ├── modules/                   # Feature documentation
│   ├── systems/                   # Infrastructure docs
│   └── integrations/              # External service docs
├── .github/workflows/ci.yml       # GitHub Actions CI
├── docs/
│   ├── ARCHITECTURE.md
│   └── SPEC.md
├── prompts/                       # Development phase prompts
├── docker-compose.yml             # Production Docker Compose
└── build.sh / build.ps1           # Nuke build scripts
```

## Custom Commands

Use these commands in Claude Code for common tasks:

| Command | Description |
|---------|-------------|
| `/feature CreateProfile` | Create a new vertical feature slice |
| `/component TransactionList in features/transactions` | Create a React component |
| `/entity ImportBatch with ProfileId, Status, CreatedAt` | Create an EF Core entity |
| `/test POST /api/profiles` | Create integration tests |
| `/rules add rule for Netflix` | Work with the rules engine |

## Development Phases

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | Foundation - Auth, Profiles, Accounts | Complete |
| 2 | Import & Rules - CSV, LLM detection, TOML engine | In Progress |
| 3 | Dashboard - Transaction views, visualizations | Planned |
| 4 | NLQ & Export - Natural language queries | Planned |
| 5 | Polish - Error handling, performance | Planned |

## Technology Stack

| Technology | Version |
|------------|---------|
| .NET | 10.0 |
| C# | 14 |
| PostgreSQL | 18 |
| React | 19 |
| Tailwind CSS | 4.x |
| Vite | 6.x |
| Node.js | 22.x |
| .NET Aspire | 9.x |
| Nuke Build | 10.x |

## Prerequisites

- .NET 10 SDK
- Node.js 22+ with pnpm
- Docker Desktop
- (Optional) Claude Code CLI

### OpenRouter API Key

Get an API key from [OpenRouter](https://openrouter.ai) and add to `.env`:

```bash
OPENROUTER_API_KEY=sk-or-v1-your-key-here
```

## Tips for Using Claude Code

### 1. Reference Phase Prompts
```
/user Let's work on Phase 2, Task 2.5: LLM Format Detection
```

### 2. Use Custom Commands
```
/feature ImportTransactions
```

### 3. Request Specific Files
```
Create the OpenRouter service in src/FinTrack.Infrastructure/Services/LlmService/OpenRouterService.cs
```

### 4. Ask for Tests
```
Add integration tests for the import endpoint we just created
```

### 5. Use Agents for Specialized Tasks
```
Acting as the backend-agent, implement the rules parser
```

## Project Conventions

### File Naming
- C#: `PascalCase.cs`
- TypeScript: `PascalCase.tsx` for components, `camelCase.ts` for utilities
- Tests: `{Feature}Tests.cs`, `{Component}.test.tsx`

### Code Style
- C#: Use records, pattern matching, file-scoped namespaces
- TypeScript: Functional components, custom hooks, Zod validation

### Git Commits
Use conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation
- `refactor:` Code refactoring
- `test:` Adding tests

## Troubleshooting

### Database Connection
```bash
# Check if PostgreSQL is running
docker compose ps

# View logs
docker compose logs db
```

### Build Errors
```bash
# Clear and restore
dotnet clean
dotnet restore
```

### Frontend Issues
```bash
cd src/FinTrack.Host/ClientApp
rm -rf node_modules pnpm-lock.yaml
pnpm install
```

## Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [Wolverine FX Documentation](https://wolverinefx.io/)
- [React 19 Documentation](https://react.dev/)
- [Tailwind CSS 4 Documentation](https://tailwindcss.com/docs)
- [PostgreSQL 18 Release Notes](https://www.postgresql.org/docs/18/release-18.html)
- [OpenRouter API](https://openrouter.ai/docs)

## License

MIT License - See LICENSE file for details.
