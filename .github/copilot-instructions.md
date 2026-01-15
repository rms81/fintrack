# Copilot Code Review Instructions

## Project Context

FinTrack is a self-hosted expense tracking system built with:
- **Backend**: .NET 10, ASP.NET Core Minimal APIs, Wolverine FX (CQRS), EF Core 10, PostgreSQL 18
- **Frontend**: React 19, TypeScript, Tailwind CSS 4, TanStack Query, React Router 7
- **Testing**: xUnit, Vitest, Playwright (E2E), Testcontainers

## Code Review Focus Areas

### Security
- Check for SQL injection, XSS, and command injection vulnerabilities
- Ensure user input is validated at system boundaries
- Verify authentication/authorization is properly enforced on endpoints
- Flag any hardcoded secrets or credentials

### C# Code Style
- Prefer records and pattern matching (functional approach)
- Use file-scoped namespaces
- Use primary constructors for simple classes
- Enable nullable reference types
- Follow naming: Commands (`CreateProfile`), Queries (`GetProfiles`), DTOs (`ProfileDto`)

### TypeScript/React Style
- Functional components only (no class components)
- Custom hooks for reusable logic
- Zod for runtime validation
- Colocation of related code

### Database Conventions
- Table names: snake_case, plural (`transactions`, `profiles`)
- Column names: snake_case (`created_at`, `profile_id`)
- Primary keys: UUIDv7
- Use PostgreSQL native arrays for tags (`text[]`)

### API Conventions
- REST with resource-oriented URLs
- Problem Details (RFC 7807) for errors
- Pagination: `?page=1&pageSize=20`

### Testing Requirements
- Integration tests should use `WebApplicationFactory` and Testcontainers
- Frontend tests use Vitest + Testing Library
- E2E tests use Playwright

## What to Flag

- Missing error handling at API boundaries
- N+1 query patterns in EF Core
- Unused imports or dead code
- Missing null checks where appropriate
- Inconsistent naming conventions
- Missing or incorrect async/await usage
- React components without proper key props in lists
- Direct DOM manipulation instead of React state

## What NOT to Flag

- Missing XML documentation on internal methods
- Using `var` for obvious types
- Single-letter variables in LINQ expressions (`x => x.Id`)
- Test method naming variations (both `Should_` and descriptive names are acceptable)
