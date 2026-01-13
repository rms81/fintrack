# FinTrack Architecture

## Overview

FinTrack is a self-hosted expense tracking application built with a modern .NET and React stack. It follows vertical slice architecture with lean DDD principles.

## Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend | ASP.NET Core | 10.0 |
| Language | C# | 14 |
| CQRS | Wolverine FX | 3.x |
| ORM | EF Core | 10.0 |
| Database | PostgreSQL | 18 |
| Frontend | React | 19 |
| Styling | Tailwind CSS | 4.x |
| Build | Vite | 6.x |
| State | TanStack Query | 5.x |
| Charts | Recharts | 2.x |
| LLM | OpenRouter API | - |
| Container | Docker | - |

## Architecture Decisions

### 1. Vertical Slice Architecture

**Decision**: Organize code by feature instead of technical layer.

**Rationale**: 
- Related code stays together
- Easier to understand and modify
- Reduces coupling between features
- Each slice is independently deployable

**Structure**:
```
Features/
├── Profiles/
│   ├── CreateProfile.cs    # Command + Handler
│   ├── GetProfiles.cs      # Query + Handler
│   └── ProfileDto.cs       # DTOs
├── Transactions/
│   ├── ImportTransactions.cs
│   ├── GetTransactions.cs
│   └── TransactionDto.cs
```

### 2. Wolverine FX for CQRS

**Decision**: Use Wolverine FX instead of MediatR.

**Rationale**:
- Native minimal API integration
- Better performance
- Built-in messaging capabilities
- Less boilerplate than MediatR
- Active development by Jeremy Miller

**Usage**:
```csharp
// Command with handler
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

### 3. PostgreSQL with Native Features

**Decision**: Use PostgreSQL-specific features.

**Rationale**:
- UUIDv7 native support
- JSONB for flexible data
- Array types for tags
- pg_trgm for fuzzy search
- Better performance for complex queries

**Features Used**:
- `uuidv7()` for IDs
- `text[]` for tags
- `jsonb` for raw data
- GIN indexes for search

### 4. BFF Pattern

**Decision**: ASP.NET serves React SPA and provides API.

**Rationale**:
- Single deployment unit
- Same-origin requests (no CORS)
- Server-side rendering possible later
- Authentication handled server-side
- Simplified infrastructure

**Structure**:
```
FinTrack.Host/
├── Program.cs          # API endpoints
├── Endpoints/          # Endpoint definitions
└── ClientApp/          # React application
    ├── src/
    └── package.json
```

### 5. TOML Rules Engine

**Decision**: Use TOML for rule definition.

**Rationale**:
- Human-readable format
- Proven in davidfowl/tally
- Easy to version control
- LLM can generate rules
- Deterministic execution

**Format**:
```toml
[netflix]
match = 'normalized("NETFLIX")'
category = "Subscriptions"
tags = ["streaming", "recurring"]
```

### 6. OpenRouter for LLM

**Decision**: Use OpenRouter as LLM gateway.

**Rationale**:
- Multiple provider support
- Easy provider switching
- Local model support (future)
- Single API interface
- Cost optimization

## Data Flow

### Import Flow
```
CSV Upload → Format Detection (LLM) → User Confirms Mapping →
Parse Transactions → Duplicate Check → Save → Apply Rules
```

### NLQ Flow
```
User Question → Build Prompt (with schema) → LLM → 
Validate SQL → Execute Query → Format Results → Render
```

### Rule Application Flow
```
Load Rules → Parse TOML → Build Expression Trees →
For each Transaction: Match Rules → First Match Wins → Update Category
```

## Security Model

### Authentication
- External OIDC provider
- JWT tokens
- Token validation in middleware

### Authorization
- Resource-based authorization
- Profile ownership checks
- Endpoint-level policies

### Data Isolation
- All queries filter by UserId
- Profile scopes all data
- No cross-user data access

## Database Schema

```
users
├── id (uuid)
├── email
├── name
└── created_at

profiles
├── id (uuid)
├── user_id (fk)
├── name
├── type
└── created_at

accounts
├── id (uuid)
├── profile_id (fk)
├── name
├── bank_name
├── currency
└── created_at

categories
├── id (uuid)
├── profile_id (fk)
├── name
├── parent_id (self fk)
├── color
├── icon
└── created_at

transactions
├── id (uuid)
├── account_id (fk)
├── date
├── description
├── normalized_description
├── amount
├── category_id (fk)
├── matched_rule_id (fk)
├── tags (text[])
├── is_manual_override
├── raw_data (jsonb)
├── import_batch_id
└── created_at

rules
├── id (uuid)
├── profile_id (fk)
├── name
├── priority
├── match_expression
├── category_id (fk)
├── tags (text[])
└── created_at
```

## Performance Considerations

### Backend
- Compiled EF queries for hot paths
- Projection queries (avoid SELECT *)
- Pagination with keyset
- Connection pooling
- Response compression

### Frontend
- Route-based code splitting
- Virtual lists for long data
- Optimistic updates
- Request deduplication
- Asset optimization

### Database
- Appropriate indexes
- Partial indexes for common filters
- GIN indexes for arrays and text search
- Query plan analysis
- Connection pooling

## Deployment

### Development
```bash
docker compose up -d db    # Start PostgreSQL
dotnet run                  # Start backend
cd ClientApp && npm run dev # Start frontend (hot reload)
```

### Production
```bash
docker compose up -d       # Start all services
```

### Environment Variables
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection
- `Llm__ApiKey` - OpenRouter API key
- `Llm__Model` - LLM model name
- `Auth__Authority` - OIDC provider URL

## Future Considerations

1. **Recurring Transactions**: Auto-detect and flag
2. **Budgeting**: Track against budgets with alerts
3. **Multi-Currency**: Handle multiple currencies
4. **Bank Integration**: Open Banking APIs
5. **Mobile**: PWA or native app
6. **Collaboration**: Share profiles
