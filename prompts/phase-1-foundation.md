# Phase 1: Foundation

## Duration: Week 1-2

## Overview
Set up the project structure, infrastructure, authentication integration, and basic profile/account management.

## Goals
1. Create solution structure with all projects
2. Configure Docker Compose with PostgreSQL 18
3. Integrate with external Auth Server (OpenID Connect)
4. Implement Profile and Account CRUD
5. Set up React app with routing

---

## Task 1.1: Solution Setup

### Prompt
```
Create the FinTrack solution structure with the following projects:

1. src/FinTrack.Host - ASP.NET Core 10 web host
2. src/FinTrack.Core - Domain entities and feature handlers
3. src/FinTrack.Infrastructure - EF Core, external services
4. tests/FinTrack.Tests.Unit - Unit tests
5. tests/FinTrack.Tests.Integration - Integration tests

Include:
- Directory.Build.props with common settings
- Global usings
- .editorconfig for code style
- .gitignore

Use .NET 10, enable nullable reference types, implicit usings, and file-scoped namespaces.
```

### Expected Output
- Solution file
- Project files with correct references
- Build configuration

---

## Task 1.2: Docker Compose

### Prompt
```
Create Docker Compose configuration for local development:

1. PostgreSQL 18 container
   - Enable pg_trgm extension
   - Health checks
   - Persistent volume

2. App container (for production)
   - Multi-stage Dockerfile
   - Build React app during image build
   - Non-root user

Include:
- docker-compose.yml
- docker-compose.override.yml (for dev)
- .env.example

The PostgreSQL container should be accessible on port 5432 locally.
```

### Expected Output
- docker-compose.yml
- Dockerfile
- Environment configuration

---

## Task 1.3: Database Setup

### Prompt
```
Set up Entity Framework Core 10 with PostgreSQL 18:

1. Create FinTrackDbContext in Infrastructure project
2. Configure connection string from environment
3. Enable snake_case naming convention
4. Create initial migration with:
   - Enable pg_trgm extension
   - Users table
   - Profiles table
   - Accounts table

Include proper indexes and foreign key relationships.

Use UUIDv7 for primary keys via PostgreSQL's uuidv7() function.
```

### Expected Output
- DbContext implementation
- Entity configurations
- Initial migration

---

## Task 1.4: Authentication Integration

### Prompt
```
Integrate with an external OpenID Connect Auth Server:

1. Configure JWT Bearer authentication in Program.cs
2. Create extension methods to extract user claims
3. Implement authorization policies
4. Add middleware to populate user context

The Auth Server provides:
- Authority URL from configuration
- Standard OIDC claims (sub, email, name)

Include:
- ICurrentUser service for accessing authenticated user
- RequireAuthorization on API endpoints
- Test authentication handler for integration tests
```

### Expected Output
- Authentication configuration
- User context service
- Test authentication handler

---

## Task 1.5: Wolverine FX Setup

### Prompt
```
Configure Wolverine FX for CQRS:

1. Add Wolverine packages to Host project
2. Configure message bus in Program.cs
3. Set up handler discovery
4. Configure HTTP integration for minimal APIs

Include:
- Example command and query
- Handler conventions
- Error handling middleware
```

### Expected Output
- Wolverine configuration
- Example handlers

---

## Task 1.6: Profile Management

### Prompt
```
Implement Profile CRUD operations:

Entities:
- Profile (Id, UserId, Name, Type, CreatedAt)
- ProfileType enum (Personal, Business)

Commands/Queries:
- CreateProfile
- GetProfiles (for current user)
- GetProfile (by id)
- UpdateProfile
- DeleteProfile

Endpoints:
- POST /api/profiles
- GET /api/profiles
- GET /api/profiles/{id}
- PUT /api/profiles/{id}
- DELETE /api/profiles/{id}

Business Rules:
- Profile names don't need to be unique
- Users can have multiple profiles
- Deleting a profile cascades to accounts and transactions

Include integration tests for all endpoints.
```

### Expected Output
- Profile entity and configuration
- All CRUD handlers
- API endpoints
- Integration tests

---

## Task 1.7: Account Management

### Prompt
```
Implement Account management:

Entity:
- Account (Id, ProfileId, Name, BankName?, Currency, CreatedAt)

Commands/Queries:
- CreateAccount
- GetAccounts (by profile)
- UpdateAccount
- DeleteAccount

Endpoints:
- POST /api/profiles/{profileId}/accounts
- GET /api/profiles/{profileId}/accounts
- PUT /api/profiles/{profileId}/accounts/{id}
- DELETE /api/profiles/{profileId}/accounts/{id}

Business Rules:
- Account must belong to a profile owned by the current user
- Currency defaults to EUR
- Deleting account cascades to transactions

Include authorization checks and integration tests.
```

### Expected Output
- Account entity and configuration
- All CRUD handlers
- API endpoints with authorization
- Integration tests

---

## Task 1.8: React App Setup

### Prompt
```
Create the React 19 frontend application:

1. Initialize with Vite
2. Configure Tailwind CSS 4 with @tailwindcss/vite
3. Set up routing with React Router 7
4. Configure TanStack Query
5. Create API client

Structure:
- src/features/ - Feature-based organization
- src/components/ui/ - Shared UI components
- src/lib/ - Utilities, API client, types
- src/hooks/ - Shared hooks

Include:
- Layout component with header and sidebar
- Profile switcher component
- Protected route wrapper
- Basic shadcn/ui components (Button, Input, Select)
```

### Expected Output
- Vite configuration
- Tailwind configuration
- React Router setup
- Base layout and components

---

## Task 1.9: Profile UI

### Prompt
```
Create the Profile management UI:

Pages:
- /profiles - List of profiles with switcher
- /profiles/new - Create profile form
- /profiles/{id}/settings - Edit profile

Components:
- ProfileCard - Display profile info
- ProfileForm - Create/edit form
- ProfileSwitcher - Header dropdown to switch profiles

Features:
- Store active profile in localStorage
- Redirect to profile selection if none active
- Show profile type badge (Personal/Business)

Use TanStack Query for data fetching and mutations.
```

### Expected Output
- Profile pages and components
- Profile switcher
- Active profile state management

---

## Task 1.10: Account UI

### Prompt
```
Create the Account management UI:

Pages:
- /accounts - List accounts for active profile
- /accounts/new - Create account form
- /accounts/{id} - Account details and edit

Components:
- AccountCard - Display account info
- AccountForm - Create/edit form
- AccountList - List with filtering

Features:
- Show account currency
- Show transaction count (placeholder for now)
- Confirm before deleting account

Include loading and error states.
```

### Expected Output
- Account pages and components
- Account list and forms

---

## Completion Criteria

Phase 1 is complete when:
- [ ] Solution builds successfully
- [ ] Docker Compose starts PostgreSQL
- [ ] Migrations run successfully
- [ ] Authentication works with JWT
- [ ] Profile CRUD works end-to-end
- [ ] Account CRUD works end-to-end
- [ ] React app loads and routes work
- [ ] Profile switcher persists selection
- [ ] All integration tests pass

## Next Phase
Proceed to Phase 2: Import & Rules
