# FinTrack - Claude Code Development Package

This package contains all the configuration, prompts, and documentation needed to develop FinTrack using Claude Code.

## Quick Start

### 1. Clone/Create Project Directory

```bash
mkdir fintrack && cd fintrack
# Copy all files from this package into the directory
```

### 2. Open in Claude Code

```bash
claude
```

Claude Code will automatically read the `CLAUDE.md` file for project context.

### 3. Start Development

Begin with Phase 1 by running:

```
/user Read prompts/phase-1-foundation.md and start Task 1.1
```

## Package Contents

```
fintrack-claude-code/
├── CLAUDE.md                      # Main project context for Claude Code
├── .claude/
│   ├── settings.json              # Claude Code settings
│   └── commands/
│       ├── feature.md             # /feature - Create feature slice
│       ├── component.md           # /component - Create React component
│       ├── entity.md              # /entity - Create EF Core entity
│       ├── test.md                # /test - Create integration tests
│       └── rules.md               # /rules - Work with rules engine
├── agents/
│   ├── backend-agent.md           # Backend development specialization
│   └── frontend-agent.md          # Frontend development specialization
├── prompts/
│   ├── phase-1-foundation.md      # Week 1-2: Setup, auth, profiles
│   ├── phase-2-import-rules.md    # Week 3-4: CSV import, rules engine
│   ├── phase-3-dashboard.md       # Week 5-6: Dashboard, visualizations
│   ├── phase-4-nlq-export.md      # Week 7-8: NLQ, export/import
│   └── phase-5-polish.md          # Week 9-10: Polish, optimization
├── docs/
│   ├── ARCHITECTURE.md            # Architecture decisions
│   └── SPEC.md                    # Full technical specification
├── docker-compose.yml             # Production Docker Compose
├── docker-compose.override.yml    # Development override
├── src/FinTrack.Host/
│   └── Dockerfile                 # Multi-stage Dockerfile
├── init-db.sql                    # PostgreSQL initialization
└── .env.example                   # Environment variables template
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

### Phase 1: Foundation (Week 1-2)
- Solution structure
- Docker setup
- Authentication
- Profile & Account CRUD
- React shell

### Phase 2: Import & Rules (Week 3-4)
- CSV upload
- LLM format detection
- Transaction import
- TOML rules engine
- Rule application

### Phase 3: Dashboard (Week 5-6)
- Transaction list
- Spending charts
- Category management
- Date filtering

### Phase 4: NLQ & Export (Week 7-8)
- Natural language queries
- SQL generation
- JSON/CSV export
- Data import

### Phase 5: Polish (Week 9-10)
- Error handling
- Performance
- Documentation
- Testing

## Technology Versions

| Technology | Version |
|------------|---------|
| .NET | 10.0 (LTS) |
| C# | 14 |
| PostgreSQL | 18 |
| React | 19 |
| Tailwind CSS | 4.x |
| Vite | 6.x |
| Node.js | 22.x |

## Environment Setup

### Prerequisites
- .NET 10 SDK
- Node.js 22+
- Docker Desktop
- Claude Code CLI

### Database

```bash
# Start PostgreSQL
docker compose up -d db

# Verify connection
docker compose exec db psql -U fintrack -d fintrack -c "SELECT version();"
```

### Development

```bash
# Backend
dotnet run --project src/FinTrack.Host

# Frontend (separate terminal)
cd src/FinTrack.Host/ClientApp
npm install
npm run dev
```

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
rm -rf node_modules
npm install
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
