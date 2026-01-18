# Changelog

All notable changes to FinTrack will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-18

### Added

#### Core Features
- Multi-profile support for separating personal and business finances
- Account management with support for multiple bank accounts per profile
- Transaction management with search, filtering, and pagination
- CSV import with auto-detection of bank statement formats
- TOML-based rules engine for automatic transaction categorization
- Hierarchical categories with icons and colors
- Interactive dashboard with spending charts and summaries
- Natural language queries (NLQ) for asking questions about your data
- JSON/CSV export and JSON import for data portability

#### User Experience
- Dark mode with system preference detection
- Skeleton loaders for smooth loading states
- Empty state components with helpful guidance
- Toast notifications for user feedback
- Global error boundary for graceful error handling

#### Performance
- Route-based code splitting with React.lazy
- Vendor chunk splitting for better caching (React, Router, Query, Recharts)
- Server-side pagination for transactions
- Database caching for categories and dashboard data
- PostgreSQL connection pooling with retry logic

#### Security
- Cookie-based authentication with ASP.NET Core Identity
- Security headers (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy)
- SQL injection prevention with parameterized queries
- Row-level security enforcement for NLQ queries

#### Observability
- Correlation ID middleware for request tracing
- Structured request logging with duration and status
- Health check endpoints (/health, /alive, /ready)
- OpenTelemetry integration for distributed tracing
- PostgreSQL health check for readiness probes

#### Developer Experience
- .NET Aspire for local development orchestration
- Comprehensive unit and integration test suites
- E2E tests with Playwright
- OpenAPI/Swagger documentation
- TypeScript throughout the React frontend

### Technical Stack
- Backend: .NET 10, ASP.NET Core Minimal APIs, Wolverine FX, EF Core
- Frontend: React 19, TanStack Query, Tailwind CSS, Recharts
- Database: PostgreSQL 17
- Infrastructure: Docker, .NET Aspire

---

## [Unreleased]

### Planned
- Budget tracking and alerts
- Recurring transaction detection
- Mobile-responsive improvements
- Additional chart visualizations
- Import format templates for common banks
