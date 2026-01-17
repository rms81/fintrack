# GitHub Copilot Instructions for FinTrack

## Project Overview

FinTrack is a self-hosted expense tracking system for individuals and sole proprietors. It features:
- Multi-profile support for managing multiple entities
- Intelligent CSV import with LLM-powered format detection
- Rule-based transaction categorization using TOML configuration
- Natural language querying capabilities
- Real-time dashboards and visualizations

### Technology Stack

**Backend:**
- .NET 10 with C# 14
- ASP.NET Core Minimal APIs
- Wolverine FX for CQRS (commands/queries)
- EF Core 10 with PostgreSQL 18
- OpenRouter API for LLM features
- .NET Aspire for orchestration

**Frontend:**
- React 19 with TypeScript
- Tailwind CSS 4 for styling
- TanStack Query for server state
- React Router 7 for routing
- Vite 6 for build tooling
- Recharts for data visualization
- Zod for validation

**Testing:**
- xUnit for C# unit/integration tests
- Vitest + Testing Library for React tests
- Playwright for E2E tests
- Testcontainers for integration tests

**Build System:**
- Nuke Build for .NET projects
- pnpm for frontend dependencies

## Development Setup

### Quick Start with .NET Aspire (Recommended)
```bash
dotnet run --project src/FinTrack.AppHost
```
- Aspire Dashboard: https://localhost:17225
- API: https://localhost:5001

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

### Build and Test Commands
```bash
# Using Nuke Build
./build.sh compile    # Build all projects
./build.sh test       # Build and run tests
./build.sh publish    # Build, test, and publish

# Frontend tests
cd src/FinTrack.Host/ClientApp
pnpm test             # Run Vitest tests
pnpm test:watch       # Watch mode
pnpm e2e              # Run Playwright E2E tests
pnpm lint             # Run ESLint
```

## Project Structure

```
fintrack/
├── src/
│   ├── FinTrack.Host/              # ASP.NET host + React SPA (BFF pattern)
│   ├── FinTrack.Core/              # Domain models + Application logic
│   ├── FinTrack.Infrastructure/    # EF Core, External services
│   ├── FinTrack.AuthServer/        # Authentication server
│   ├── FinTrack.AppHost/           # .NET Aspire orchestrator
│   └── FinTrack.ServiceDefaults/   # Shared service configuration
├── tests/
│   ├── FinTrack.Tests.Integration/ # Integration tests with Testcontainers
│   └── FinTrack.Tests.Unit/        # Unit tests
├── build/                          # Nuke build project
└── docs/                           # Architecture and specifications
```

## Architecture Pattern

**Vertical Slice Architecture**: Organize code by feature, not technical layer.
- Each feature contains its commands, queries, handlers, and DTOs
- Example: `Features/Profiles/CreateProfile.cs` contains command, handler, and related types
- Reduces coupling between features
- Makes code easier to understand and modify

## Coding Conventions

### C# (.NET) Guidelines

**File Organization:**
- Use file-scoped namespaces (not block-scoped)
- One type per file, named to match the type
- Commands: `CreateProfile.cs`, Queries: `GetProfiles.cs`, DTOs: `ProfileDto.cs`

**Code Style:**
- Prefer records over classes for DTOs and value objects
- Use primary constructors for simple classes
- Use pattern matching and switch expressions
- Enable nullable reference types (enabled project-wide)
- Use `var` when type is apparent
- Single-letter variables in LINQ are acceptable: `x => x.Id`

**CQRS with Wolverine:**
```csharp
// Command with handler in same file
public record CreateProfile(Guid UserId, string Name);

public static class CreateProfileHandler
{
    public static async Task<ProfileDto> Handle(
        CreateProfile command,
        FinTrackDbContext db,
        CancellationToken ct)
    {
        // Implementation
    }
}
```

**Async/Await:**
- Always use async/await for I/O operations
- Include CancellationToken in all async methods
- Never use `.Result` or `.Wait()`

**Error Handling:**
- Use Problem Details (RFC 7807) for API errors
- Validate at system boundaries (API endpoints)
- Use FluentValidation for complex validation rules

**Naming Conventions:**
- Private fields: `_camelCase` with underscore prefix
- Methods, properties: `PascalCase`
- Local variables, parameters: `camelCase`
- Constants: `PascalCase`

### TypeScript/React Guidelines

**Component Structure:**
- Use functional components only (no class components)
- Use custom hooks for reusable logic
- Colocate related code (component, styles, tests)
- Export components as named exports

**Type Safety:**
- Use Zod for runtime validation of API responses
- Define TypeScript interfaces for all data shapes
- Avoid `any` type; use `unknown` if necessary
- Enable strict mode in tsconfig.json

**State Management:**
- Use TanStack Query for server state
- Use React hooks (useState, useReducer) for local state
- Avoid prop drilling; use composition instead

**Styling:**
- Use Tailwind CSS utility classes
- Use `clsx` or `tailwind-merge` for conditional classes
- Follow mobile-first responsive design

**Common Patterns:**
```typescript
// Custom hook for data fetching
export function useProfiles() {
  return useQuery({
    queryKey: ['profiles'],
    queryFn: () => fetch('/api/profiles').then(r => r.json())
  });
}

// Zod validation
const ProfileSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1),
});
```

### Database Conventions

**Naming:**
- Table names: `snake_case`, plural (`transactions`, `profiles`)
- Column names: `snake_case` (`created_at`, `profile_id`)
- Foreign keys: `{table_name}_id` (e.g., `profile_id`)

**Keys and Types:**
- Primary keys: UUID (using PostgreSQL's `gen_random_uuid()`)
- Timestamps: `timestamptz` for all datetime values
- Arrays: Use PostgreSQL native arrays (`text[]`) for tags
- JSON: Use `jsonb` for flexible/raw data storage

**Indexes:**
- Add indexes for foreign keys
- Use GIN indexes for JSONB and array columns
- Consider partial indexes for filtered queries

### API Conventions

**REST Principles:**
- Use resource-oriented URLs: `/api/profiles/{id}/transactions`
- HTTP methods: GET (read), POST (create), PUT (update), DELETE (remove)
- Return Problem Details (RFC 7807) for errors
- Use proper status codes: 200, 201, 204, 400, 401, 403, 404, 500

**Pagination:**
- Query params: `?page=1&pageSize=20`
- Response headers: `X-Total-Count`, `Link` (rel=next/prev)

**Validation:**
- Validate all input at API boundaries
- Return 400 with validation errors in Problem Details format

## Testing Requirements

**C# Testing:**
- Use xUnit for all tests
- Name tests descriptively (both `Should_DoSomething_When_Condition` and `DoSomething_ReturnsExpectedResult` are acceptable)
- Integration tests: Use `WebApplicationFactory` and Testcontainers for PostgreSQL
- Arrange-Act-Assert pattern for test structure
- One assertion per test (where practical)

**React Testing:**
- Use Vitest + Testing Library for component tests
- Use Playwright for E2E tests
- Test user behavior, not implementation details
- Mock API calls in component tests, use real API in E2E tests

**Test Coverage:**
- Aim for high coverage on business logic
- Don't test framework code or trivial getters/setters
- Focus on edge cases and error paths

## Common Patterns and Practices

### Use These Patterns

✅ **Vertical Slices** - Keep feature code together
```
Features/Profiles/
├── CreateProfile.cs
├── GetProfiles.cs
└── ProfileDto.cs
```

✅ **Wolverine Handlers** - Use static handler methods
```csharp
public static class Handler
{
    public static async Task<TResult> Handle(TCommand cmd, Dependencies...)
}
```

✅ **Problem Details** - For all API errors
```csharp
return Results.Problem("Not found", statusCode: 404);
```

✅ **PostgreSQL Features** - UUID, JSONB, arrays, GIN indexes

✅ **React Composition** - Small, focused components

✅ **Custom Hooks** - Extract reusable logic

### Avoid These Anti-Patterns

❌ **N+1 Queries** - Use `.Include()` or projections in EF Core

❌ **Hardcoded Secrets** - Use configuration/environment variables

❌ **Direct DOM Manipulation** - Use React state instead

❌ **Class Components** - Use functional components

❌ **Blocking Async** - Never use `.Result` or `.Wait()`

❌ **Missing Error Handling** - Always handle errors at API boundaries

❌ **Mutation in React** - Use immutable updates

## Security Guidelines

**Input Validation:**
- Validate all user input at API boundaries
- Use Zod for TypeScript validation
- Use FluentValidation or data annotations for C#
- Sanitize data before database operations

**Authentication/Authorization:**
- Verify authentication on all protected endpoints
- Check authorization before data access
- Never trust client-side validation alone

**Secrets Management:**
- Never hardcode API keys, connection strings, or passwords
- Use .NET Configuration system
- Use environment variables for secrets
- Add sensitive patterns to .gitignore

**SQL Injection Prevention:**
- Always use parameterized queries (EF Core handles this)
- Never concatenate user input into SQL strings
- Use Dapper parameters if raw SQL is needed

**XSS Prevention:**
- React escapes by default - don't use `dangerouslySetInnerHTML` unless necessary
- Sanitize any HTML content from users
- Set proper Content-Security-Policy headers

**Command Injection:**
- Never execute shell commands with user input
- Validate and sanitize file paths
- Use safe APIs instead of shell execution

## Code Review Focus Areas

**Security:**
- Check for SQL injection, XSS, and command injection vulnerabilities
- Ensure user input is validated at system boundaries
- Verify authentication/authorization is properly enforced on endpoints
- Flag any hardcoded secrets or credentials

**Code Quality:**
- Missing error handling at API boundaries
- N+1 query patterns in EF Core
- Unused imports or dead code
- Missing null checks where appropriate
- Inconsistent naming conventions
- Missing or incorrect async/await usage
- React components without proper key props in lists
- Direct DOM manipulation instead of React state

**What NOT to Flag:**
- Missing XML documentation on internal methods
- Using `var` for obvious types
- Single-letter variables in LINQ expressions (`x => x.Id`)
- Test method naming variations (both `Should_` and descriptive names are acceptable)

## Additional Resources

**Documentation:**
- Architecture: See `docs/ARCHITECTURE.md`
- Specifications: See `docs/SPEC.md`
- README: See `README.md` for quick start guide

**External References:**
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [Wolverine FX Documentation](https://wolverinefx.io/)
- [React 19 Documentation](https://react.dev/)
- [Tailwind CSS 4 Documentation](https://tailwindcss.com/docs)
- [PostgreSQL 18 Release Notes](https://www.postgresql.org/docs/18/release-18.html)
