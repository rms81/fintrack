# Expense Tracker Technical Specification

## Project: FinTrack

**Version:** 1.0  
**Date:** January 2026  
**Stack:** .NET 9, React, PostgreSQL, Wolverine FX, Entity Framework Core

---

## 1. Executive Summary

FinTrack is a self-hosted expense tracking system designed for individuals and sole proprietors who need to segregate personal and business finances. The system imports bank statements (CSV), applies deterministic categorization rules (TOML-based), presents data through an interactive dashboard, and supports natural language querying.

### Key Differentiators

- **Multi-profile support**: Logical separation between personal and business accounts
- **LLM-assisted import**: Auto-detect CSV formats with user confirmation
- **Deterministic rules engine**: TOML-based categorization inspired by davidfowl/tally
- **Natural language queries**: Ask questions about your data, translated to SQL
- **Export/Import**: Full data portability between instances

---

## 2. Architecture Overview

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Docker Compose                          │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    ASP.NET Host                          │   │
│  │  ┌─────────────┐  ┌──────────────────────────────────┐  │   │
│  │  │   React     │  │         Minimal API              │  │   │
│  │  │   SPA       │◄─┤         (BFF Pattern)            │  │   │
│  │  │  Tailwind   │  │                                  │  │   │
│  │  └─────────────┘  └──────────────┬───────────────────┘  │   │
│  │                                  │                       │   │
│  │                    ┌─────────────▼───────────────┐      │   │
│  │                    │      Wolverine FX           │      │   │
│  │                    │   (Commands & Queries)      │      │   │
│  │                    └─────────────┬───────────────┘      │   │
│  │                                  │                       │   │
│  │  ┌───────────────┬───────────────┼───────────────┐      │   │
│  │  │               │               │               │      │   │
│  │  ▼               ▼               ▼               ▼      │   │
│  │ Rules       Import          Domain          NLQ         │   │
│  │ Engine      Engine          Logic         Service       │   │
│  │ (TOML)     (CSV/LLM)                     (OpenRouter)   │   │
│  │                                                          │   │
│  └──────────────────────────┬───────────────────────────────┘   │
│                             │                                    │
│                             ▼                                    │
│                    ┌────────────────┐                           │
│                    │   PostgreSQL   │                           │
│                    │   (EF Core)    │                           │
│                    └────────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Project Structure (Vertical Slicing)

```
src/
├── FinTrack.Host/                    # ASP.NET host, serves React SPA
│   ├── Program.cs                    # Minimal API endpoints
│   ├── ClientApp/                    # React application
│   │   ├── src/
│   │   │   ├── features/             # Feature-based organization
│   │   │   │   ├── dashboard/
│   │   │   │   ├── transactions/
│   │   │   │   ├── import/
│   │   │   │   ├── rules/
│   │   │   │   ├── profiles/
│   │   │   │   └── nlq/
│   │   │   ├── components/           # Shared UI components
│   │   │   └── lib/                  # Utilities, API client
│   │   └── package.json
│   └── FinTrack.Host.csproj
│
├── FinTrack.Core/                    # Domain + Application logic
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Events/
│   ├── Features/                     # Vertical slices (handlers)
│   │   ├── Transactions/
│   │   ├── Import/
│   │   ├── Rules/
│   │   ├── Profiles/
│   │   ├── Export/
│   │   └── NaturalLanguage/
│   └── FinTrack.Core.csproj
│
├── FinTrack.Infrastructure/          # EF Core, External services
│   ├── Persistence/
│   │   ├── FinTrackDbContext.cs
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Services/
│   │   ├── RulesEngine/
│   │   ├── ImportEngine/
│   │   └── LlmService/
│   └── FinTrack.Infrastructure.csproj
│
└── tests/
    ├── FinTrack.Tests.Integration/   # Endpoint tests
    └── FinTrack.Tests.Unit/          # Logic tests
```

### 2.3 Key Technology Decisions

| Concern | Choice | Rationale |
|---------|--------|-----------|
| CQRS | Wolverine FX | Built-in messaging, minimal ceremony, great with minimal APIs |
| ORM | EF Core (Code-First) | PostgreSQL support, migrations, LINQ for NLQ |
| Rules Format | TOML | Human-readable, proven in tally, easy to parse |
| LLM Gateway | OpenRouter | Flexibility to switch providers, local model support later |
| Frontend | React + Tailwind | Modern, fast development, good charting ecosystem |
| BFF Pattern | Minimal APIs | Type-safe, co-located with React build |

---

## 3. Domain Model

### 3.1 Core Entities

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│    User     │       │   Profile   │       │   Account   │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id          │1     *│ Id          │1     *│ Id          │
│ Email       │───────│ UserId      │───────│ ProfileId   │
│ Name        │       │ Name        │       │ Name        │
│ CreatedAt   │       │ Type        │       │ BankName    │
└─────────────┘       │ CreatedAt   │       │ Currency    │
                      └──────┬──────┘       │ CreatedAt   │
                             │              └──────┬──────┘
                             │                     │
                      ┌──────▼──────┐              │
                      │    Rule     │              │
                      ├─────────────┤              │
                      │ Id          │              │
                      │ ProfileId   │              │
                      │ Name        │              │
                      │ Priority    │              │
                      │ MatchExpr   │              │
                      │ Category    │              │
                      │ Subcategory │              │
                      │ Tags        │              │
                      └─────────────┘              │
                                                  │
                      ┌───────────────────────────┘
                      │
               ┌──────▼──────┐       ┌─────────────┐
               │ Transaction │       │  Category   │
               ├─────────────┤       ├─────────────┤
               │ Id          │      *│ Id          │
               │ AccountId   │───────│ ProfileId   │
               │ ExternalId  │       │ Name        │
               │ Date        │       │ ParentId    │
               │ Description │       │ Icon        │
               │ Amount      │       │ Color       │
               │ CategoryId  │       └─────────────┘
               │ RuleId      │
               │ Tags        │       ┌─────────────┐
               │ IsManual    │       │ImportFormat │
               │ RawData     │       ├─────────────┤
               │ ImportBatch │       │ Id          │
               │ CreatedAt   │       │ ProfileId   │
               └─────────────┘       │ Name        │
                                     │ BankName    │
                                     │ Mapping     │
                                     │ CreatedAt   │
                                     └─────────────┘
```

### 3.2 Entity Definitions

```csharp
// Domain/Entities/User.cs
public sealed class User
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; init; }
    
    private readonly List<Profile> _profiles = [];
    public IReadOnlyCollection<Profile> Profiles => _profiles.AsReadOnly();
}

// Domain/Entities/Profile.cs
public sealed class Profile
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public required string Name { get; set; }
    public ProfileType Type { get; set; }  // Personal, Business
    public DateTime CreatedAt { get; init; }
    
    private readonly List<Account> _accounts = [];
    private readonly List<Rule> _rules = [];
    private readonly List<Category> _categories = [];
    
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();
    public IReadOnlyCollection<Rule> Rules => _rules.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();
}

// Domain/Entities/Transaction.cs
public sealed class Transaction
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string? ExternalId { get; init; }  // Bank's transaction ID
    public DateOnly Date { get; init; }
    public required string Description { get; init; }
    public required string NormalizedDescription { get; init; }
    public decimal Amount { get; init; }
    public Guid? CategoryId { get; set; }
    public Guid? MatchedRuleId { get; set; }
    public List<string> Tags { get; set; } = [];
    public bool IsManualOverride { get; set; }
    public string? RawData { get; init; }  // Original CSV row as JSON
    public Guid ImportBatchId { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Domain/Entities/Rule.cs
public sealed class Rule
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public required string Name { get; init; }
    public int Priority { get; set; }
    public required string MatchExpression { get; init; }  // e.g., normalized("NETFLIX")
    public Guid CategoryId { get; init; }
    public string? Subcategory { get; set; }
    public List<string> Tags { get; set; } = [];
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; init; }
}

// Domain/Entities/ImportFormat.cs
public sealed class ImportFormat
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public required string Name { get; init; }
    public string? BankName { get; init; }
    public required ImportMapping Mapping { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Domain/ValueObjects/ImportMapping.cs
public sealed record ImportMapping(
    int DateColumn,
    string DateFormat,
    int DescriptionColumn,
    int AmountColumn,
    int? CreditColumn,      // Some banks split debit/credit
    int? DebitColumn,
    bool HasHeader,
    char Delimiter,
    string? Encoding,
    bool AmountSignInverted  // Some banks use positive for debits
);
```

### 3.3 Value Objects

```csharp
// Domain/ValueObjects/Money.cs
public readonly record struct Money(decimal Amount, string Currency = "EUR")
{
    public static Money Zero(string currency = "EUR") => new(0, currency);
    public Money Negate() => this with { Amount = -Amount };
}

// Domain/ValueObjects/DateRange.cs
public readonly record struct DateRange(DateOnly Start, DateOnly End)
{
    public bool Contains(DateOnly date) => date >= Start && date <= End;
    
    public static DateRange CurrentMonth() =>
        new(
            new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
            DateOnly.FromDateTime(DateTime.Today)
        );
}

// Domain/Enums/ProfileType.cs
public enum ProfileType
{
    Personal,
    Business
}
```

---

## 4. Feature Breakdown (Epics & User Stories)

### Epic 1: User & Profile Management

#### US-1.1: User Registration
**As a** new user  
**I want to** create an account  
**So that** I can start tracking my expenses

**Acceptance Criteria:**
- User provides email, name, and password
- Email must be unique
- Default "Personal" profile is created automatically
- User is logged in after registration

**Tasks:**
- [ ] Create User and Profile entities with EF Core configuration
- [ ] Implement RegisterUser command handler (Wolverine)
- [ ] Create POST /api/auth/register endpoint
- [ ] Create registration form React component
- [ ] Add integration test for registration flow

---

#### US-1.2: Profile Management
**As a** user  
**I want to** create and manage multiple profiles  
**So that** I can separate personal and business expenses

**Acceptance Criteria:**
- User can create new profiles with name and type
- User can rename profiles
- User can delete profiles (with confirmation, cascades data)
- User can switch between profiles in the UI

**Tasks:**
- [ ] Implement CreateProfile, UpdateProfile, DeleteProfile handlers
- [ ] Create CRUD endpoints for profiles
- [ ] Create profile switcher component in header
- [ ] Create profile settings page
- [ ] Add integration tests

---

### Epic 2: Account Management

#### US-2.1: Create Bank Account
**As a** user  
**I want to** add bank accounts to a profile  
**So that** I can import transactions from different sources

**Acceptance Criteria:**
- Account has name, bank name (optional), and currency
- Account belongs to exactly one profile
- Multiple accounts can exist per profile

**Tasks:**
- [ ] Create Account entity with EF configuration
- [ ] Implement CreateAccount, UpdateAccount, DeleteAccount handlers
- [ ] Create account management UI
- [ ] Add integration tests

---

### Epic 3: CSV Import

#### US-3.1: Upload CSV File
**As a** user  
**I want to** upload a CSV bank statement  
**So that** I can import my transactions

**Acceptance Criteria:**
- Accept CSV files up to 10MB
- Show upload progress
- File is stored temporarily for processing

**Tasks:**
- [ ] Create file upload endpoint with size validation
- [ ] Implement temporary file storage
- [ ] Create drag-and-drop upload component
- [ ] Add file type validation

---

#### US-3.2: LLM-Assisted Format Detection
**As a** user  
**I want** the system to auto-detect my CSV format  
**So that** I don't have to manually configure column mappings

**Acceptance Criteria:**
- System analyzes first 10 rows of CSV
- LLM suggests column mappings (date, description, amount)
- User sees preview with detected mappings
- User can adjust mappings before confirming
- Successful mapping can be saved as ImportFormat for reuse

**Tasks:**
- [ ] Create CSV analysis service (detect delimiters, headers)
- [ ] Implement LLM prompt for format detection
- [ ] Create OpenRouter integration service
- [ ] Build import preview UI with editable mappings
- [ ] Implement SaveImportFormat handler
- [ ] Add unit tests for CSV parsing
- [ ] Add integration test for import flow

---

#### US-3.3: Import Transactions
**As a** user  
**I want to** confirm and import detected transactions  
**So that** they appear in my account

**Acceptance Criteria:**
- Transactions are created with ImportBatchId for tracking
- Duplicate detection by ExternalId or (Date + Amount + Description)
- User sees import summary (new, duplicates, errors)
- Rules are applied automatically after import

**Tasks:**
- [ ] Implement ImportTransactions command handler
- [ ] Create duplicate detection logic
- [ ] Trigger rule engine after import
- [ ] Build import confirmation UI with summary
- [ ] Add integration tests

---

### Epic 4: Rules Engine

#### US-4.1: View and Edit Rules (TOML)
**As a** user  
**I want to** define categorization rules in TOML format  
**So that** transactions are automatically categorized

**Acceptance Criteria:**
- Rules are stored as TOML text per profile
- Syntax validation with helpful error messages
- Monaco editor with TOML syntax highlighting
- Preview which transactions would match a rule

**Tasks:**
- [ ] Create TOML parser for rules (use Tomlyn library)
- [ ] Implement rule validation
- [ ] Create rules editor page with Monaco
- [ ] Add rule preview/test functionality
- [ ] Store rules in database (Rules table from parsed TOML)

---

#### US-4.2: Rule Matching Functions
**As a** user  
**I want** flexible matching functions  
**So that** I can handle various transaction patterns

**Supported Functions (tally-compatible):**
- `normalized(pattern)` - Ignores spaces, punctuation, case
- `contains(pattern)` - Substring match
- `startswith(pattern)` - Prefix match
- `anyof(p1, p2, ...)` - Match any pattern
- `regex(pattern)` - Regular expression
- `amount > X`, `amount < X` - Amount conditions
- `and`, `or` - Logical operators

**Tasks:**
- [ ] Implement expression parser (consider Sprache or custom)
- [ ] Create matcher functions
- [ ] Add normalization logic for descriptions
- [ ] Unit test each function
- [ ] Document syntax in UI

---

#### US-4.3: Apply Rules to Transactions
**As a** user  
**I want** rules to automatically categorize transactions  
**So that** I don't have to manually categorize each one

**Acceptance Criteria:**
- Rules are applied in priority order
- First matching rule wins
- Matched rule ID is stored on transaction
- Manual overrides are preserved (not re-categorized)

**Tasks:**
- [ ] Implement ApplyRules service
- [ ] Create ReapplyRules command (for bulk re-categorization)
- [ ] Add "Apply Rules" button in UI
- [ ] Show rule match indicator on transactions

---

### Epic 5: Categories

#### US-5.1: Manage Categories
**As a** user  
**I want to** create and organize categories  
**So that** I can classify my expenses meaningfully

**Acceptance Criteria:**
- Categories are hierarchical (parent/child)
- Each category has name, optional icon, optional color
- Default categories are created with new profile
- Categories are profile-specific

**Tasks:**
- [ ] Create Category entity with self-referencing FK
- [ ] Implement CRUD handlers
- [ ] Seed default categories (Food, Transport, Shopping, etc.)
- [ ] Create category management UI
- [ ] Add drag-and-drop reordering

---

#### US-5.2: Manual Transaction Categorization
**As a** user  
**I want to** manually categorize transactions  
**So that** I can correct or override automatic categorization

**Acceptance Criteria:**
- Can select category from dropdown
- Can add/remove tags
- Manual changes set IsManualOverride = true
- Option to create rule from manual categorization

**Tasks:**
- [ ] Create UpdateTransactionCategory handler
- [ ] Build transaction edit modal
- [ ] Add "Create Rule from This" action
- [ ] Preserve manual overrides during rule reapplication

---

### Epic 6: Dashboard

#### US-6.1: Profile Dashboard
**As a** user  
**I want to** see a dashboard for my selected profile  
**So that** I can understand my spending at a glance

**Visualizations:**
- Spending by category (pie/donut chart)
- Spending over time (line/area chart)
- Top merchants (bar chart)
- Recent transactions list
- Month-over-month comparison
- Account balances summary

**Filters:**
- Date range picker
- Account filter (multi-select)
- Category filter

**Tasks:**
- [ ] Create dashboard data query handlers
- [ ] Implement date range aggregation queries
- [ ] Build dashboard layout with Tailwind
- [ ] Integrate Recharts for visualizations
- [ ] Add filter controls
- [ ] Optimize queries with proper indexes

---

#### US-6.2: Transaction List View
**As a** user  
**I want to** browse and search my transactions  
**So that** I can find specific entries

**Acceptance Criteria:**
- Paginated list with infinite scroll or pagination
- Search by description
- Filter by category, account, date range, tags
- Sort by date, amount
- Quick actions (edit category, view details)

**Tasks:**
- [ ] Create GetTransactions query with filters
- [ ] Build transaction list component
- [ ] Add search and filter UI
- [ ] Implement pagination
- [ ] Add bulk actions (categorize selected)

---

### Epic 7: Natural Language Queries

#### US-7.1: Ask Questions About Data
**As a** user  
**I want to** ask questions in natural language  
**So that** I can get insights without writing queries

**Example Questions:**
- "How much did I spend on food last month?"
- "What are my top 5 expenses this year?"
- "Show me all Netflix transactions"
- "Compare my spending in December vs November"
- "What's my average monthly spend on groceries?"

**Acceptance Criteria:**
- Input field for natural language query
- LLM translates to SQL (parameterized, read-only)
- Results displayed in appropriate format (table, chart, single value)
- Query is validated before execution (no mutations)
- Show generated SQL for transparency (collapsible)

**Tasks:**
- [ ] Create NLQ prompt template with schema context
- [ ] Implement query translation via OpenRouter
- [ ] Add SQL validation (whitelist SELECT only)
- [ ] Execute query via EF Core raw SQL
- [ ] Build results renderer (auto-detect: scalar, table, chart)
- [ ] Create NLQ input component with examples
- [ ] Add conversation history for follow-up questions

---

#### US-7.2: Budget Suggestions
**As a** user  
**I want to** ask for budget recommendations  
**So that** I can plan my finances

**Acceptance Criteria:**
- LLM analyzes spending patterns
- Suggests realistic budget per category
- Shows comparison with actual spending
- Can save suggested budget (future epic)

**Tasks:**
- [ ] Create budget analysis prompt
- [ ] Aggregate historical data for context
- [ ] Build budget suggestion UI
- [ ] (Future) Implement budget tracking

---

### Epic 8: Export/Import

#### US-8.1: Export Data (JSON)
**As a** user  
**I want to** export all my data in JSON format  
**So that** I can import it to another instance

**Export Includes:**
- Profile with settings
- Accounts
- Categories (hierarchy)
- Rules (TOML)
- Transactions
- Import formats

**Tasks:**
- [ ] Create ExportProfile query handler
- [ ] Generate timestamped JSON file
- [ ] Add download endpoint
- [ ] Build export UI with options

---

#### US-8.2: Export Data (CSV/Excel)
**As a** user  
**I want to** export transactions to CSV  
**So that** I can use them in spreadsheets

**Tasks:**
- [ ] Create CSV export handler
- [ ] Allow column selection
- [ ] Add date range filter
- [ ] Build export modal

---

#### US-8.3: Import Data (JSON)
**As a** user  
**I want to** import data from another instance  
**So that** I can migrate or restore my data

**Acceptance Criteria:**
- Upload JSON export file
- Preview what will be imported
- Handle ID conflicts (regenerate IDs)
- Preserve relationships

**Tasks:**
- [ ] Create ImportProfile command handler
- [ ] Validate JSON structure
- [ ] Remap IDs during import
- [ ] Build import wizard UI

---

## 5. API Design (Minimal APIs)

### 5.1 Endpoint Overview

```
Authentication
  POST   /api/auth/register
  POST   /api/auth/login
  POST   /api/auth/logout
  GET    /api/auth/me

Profiles
  GET    /api/profiles
  POST   /api/profiles
  GET    /api/profiles/{id}
  PUT    /api/profiles/{id}
  DELETE /api/profiles/{id}

Accounts
  GET    /api/profiles/{profileId}/accounts
  POST   /api/profiles/{profileId}/accounts
  PUT    /api/profiles/{profileId}/accounts/{id}
  DELETE /api/profiles/{profileId}/accounts/{id}

Categories
  GET    /api/profiles/{profileId}/categories
  POST   /api/profiles/{profileId}/categories
  PUT    /api/profiles/{profileId}/categories/{id}
  DELETE /api/profiles/{profileId}/categories/{id}

Transactions
  GET    /api/profiles/{profileId}/transactions
  GET    /api/profiles/{profileId}/transactions/{id}
  PUT    /api/profiles/{profileId}/transactions/{id}
  DELETE /api/profiles/{profileId}/transactions/{id}
  POST   /api/profiles/{profileId}/transactions/bulk-categorize

Import
  POST   /api/profiles/{profileId}/import/upload
  POST   /api/profiles/{profileId}/import/analyze
  POST   /api/profiles/{profileId}/import/confirm
  GET    /api/profiles/{profileId}/import-formats
  POST   /api/profiles/{profileId}/import-formats
  DELETE /api/profiles/{profileId}/import-formats/{id}

Rules
  GET    /api/profiles/{profileId}/rules/toml
  PUT    /api/profiles/{profileId}/rules/toml
  POST   /api/profiles/{profileId}/rules/validate
  POST   /api/profiles/{profileId}/rules/apply
  POST   /api/profiles/{profileId}/rules/test

Dashboard
  GET    /api/profiles/{profileId}/dashboard/summary
  GET    /api/profiles/{profileId}/dashboard/spending-by-category
  GET    /api/profiles/{profileId}/dashboard/spending-over-time
  GET    /api/profiles/{profileId}/dashboard/top-merchants

Natural Language
  POST   /api/profiles/{profileId}/nlq/query
  POST   /api/profiles/{profileId}/nlq/budget-suggestion

Export
  GET    /api/profiles/{profileId}/export/json
  GET    /api/profiles/{profileId}/export/csv
  POST   /api/import/json
```

### 5.2 Example Endpoint Implementation

```csharp
// Program.cs (excerpt)
var app = builder.Build();

// Profile endpoints
var profiles = app.MapGroup("/api/profiles")
    .RequireAuthorization();

profiles.MapGet("/", async (IMessageBus bus, ClaimsPrincipal user) =>
{
    var userId = user.GetUserId();
    var result = await bus.InvokeAsync<IReadOnlyList<ProfileDto>>(
        new GetProfiles(userId));
    return Results.Ok(result);
});

profiles.MapPost("/", async (CreateProfileRequest request, IMessageBus bus, ClaimsPrincipal user) =>
{
    var command = new CreateProfile(user.GetUserId(), request.Name, request.Type);
    var result = await bus.InvokeAsync<ProfileDto>(command);
    return Results.Created($"/api/profiles/{result.Id}", result);
});

// Transaction query with filters
profiles.MapGet("/{profileId:guid}/transactions", 
    async (Guid profileId, 
           [AsParameters] TransactionQueryParams query,
           IMessageBus bus) =>
{
    var result = await bus.InvokeAsync<PagedResult<TransactionDto>>(
        new GetTransactions(profileId, query));
    return Results.Ok(result);
});

// NLQ endpoint
profiles.MapPost("/{profileId:guid}/nlq/query",
    async (Guid profileId, NlqRequest request, IMessageBus bus) =>
{
    var result = await bus.InvokeAsync<NlqResult>(
        new ExecuteNlq(profileId, request.Question));
    return Results.Ok(result);
});
```

### 5.3 Request/Response DTOs

```csharp
// Requests
public record CreateProfileRequest(string Name, ProfileType Type);
public record UpdateTransactionRequest(Guid? CategoryId, List<string>? Tags);
public record NlqRequest(string Question, Guid? ConversationId);
public record ImportAnalyzeRequest(string FileName, Guid? FormatId);
public record ImportConfirmRequest(Guid BatchId, Guid AccountId, bool SaveFormat, string? FormatName);

// Responses
public record ProfileDto(Guid Id, string Name, ProfileType Type, int AccountCount);
public record TransactionDto(
    Guid Id, 
    DateOnly Date, 
    string Description, 
    decimal Amount,
    string? CategoryName,
    string? CategoryColor,
    List<string> Tags,
    bool IsManualOverride,
    string? MatchedRuleName
);
public record NlqResult(
    string Question,
    string GeneratedSql,
    NlqResultType ResultType,  // Scalar, Table, Error
    object? Data,
    string? ErrorMessage
);
public record PagedResult<T>(
    IReadOnlyList<T> Items, 
    int TotalCount, 
    int Page, 
    int PageSize
);
```

---

## 6. Database Schema

### 6.1 EF Core Configuration

```csharp
// Infrastructure/Persistence/Configurations/TransactionConfiguration.cs
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(t => t.NormalizedDescription)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);
            
        builder.Property(t => t.Tags)
            .HasColumnType("text[]");  // PostgreSQL array
            
        builder.Property(t => t.RawData)
            .HasColumnType("jsonb");
            
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.Date);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => new { t.AccountId, t.Date, t.Amount, t.Description })
            .HasDatabaseName("ix_transactions_duplicate_check");
            
        // Full-text search index
        builder.HasIndex(t => t.NormalizedDescription)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
```

### 6.2 Migration Example

```csharp
// Initial migration (generated, then customized)
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable extensions
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                email = table.Column<string>(maxLength: 255, nullable: false),
                name = table.Column<string>(maxLength: 100, nullable: false),
                password_hash = table.Column<string>(nullable: false),
                created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });
            
        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "users",
            column: "email",
            unique: true);
            
        // ... other tables
    }
}
```

### 6.3 PostgreSQL-Specific Features Used

- **JSONB**: For RawData (original CSV row), flexible metadata
- **Array types**: For Tags (text[])
- **pg_trgm extension**: For fuzzy text search on descriptions
- **GIN indexes**: For array containment queries and trigram search

---

## 7. Rules Engine Implementation

### 7.1 TOML Rule Format

```toml
# rules.toml

[netflix]
match = 'normalized("NETFLIX")'
category = "Subscriptions"
subcategory = "Streaming"
tags = ["entertainment", "recurring"]

[amazon_large]
match = 'anyof("AMAZON", "AMZN") and amount > 100'
category = "Shopping"
subcategory = "Online"
tags = ["online"]

[uber_rides]
match = 'regex("UBER\\s(?!EATS)")'
category = "Transport"
subcategory = "Rideshare"

[groceries]
match = 'anyof("CONTINENTE", "PINGO DOCE", "LIDL", "ALDI", "MERCADONA")'
category = "Food"
subcategory = "Groceries"
tags = ["essential"]

[restaurants]
match = 'contains("REST") or contains("CAFE") or contains("PIZZA")'
category = "Food"
subcategory = "Restaurants"
```

### 7.2 Rule Parser Implementation

```csharp
// Infrastructure/Services/RulesEngine/RuleParser.cs
public sealed class RuleParser
{
    public IReadOnlyList<ParsedRule> Parse(string toml)
    {
        var model = Toml.ToModel(toml);
        var rules = new List<ParsedRule>();
        var priority = 0;
        
        foreach (var (name, value) in model)
        {
            if (value is not TomlTable table) continue;
            
            var matchExpr = table.GetString("match") 
                ?? throw new RuleParseException($"Rule '{name}' missing 'match'");
            var category = table.GetString("category")
                ?? throw new RuleParseException($"Rule '{name}' missing 'category'");
                
            rules.Add(new ParsedRule(
                Name: name,
                Priority: priority++,
                MatchExpression: ParseExpression(matchExpr),
                Category: category,
                Subcategory: table.GetString("subcategory"),
                Tags: table.GetStringArray("tags") ?? []
            ));
        }
        
        return rules;
    }
    
    private IMatchExpression ParseExpression(string expr)
    {
        // Tokenize and parse expression
        var tokens = Tokenize(expr);
        return ParseOr(tokens);
    }
    
    // ... expression parsing implementation
}

// Match expression types
public interface IMatchExpression
{
    bool Matches(TransactionContext ctx);
}

public record NormalizedMatch(string Pattern) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        ctx.NormalizedDescription.Contains(
            Normalize(Pattern), 
            StringComparison.OrdinalIgnoreCase);
            
    private static string Normalize(string s) =>
        Regex.Replace(s.ToUpperInvariant(), @"[\s\-_.,]", "");
}

public record ContainsMatch(string Pattern) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        ctx.Description.Contains(Pattern, StringComparison.OrdinalIgnoreCase);
}

public record AnyOfMatch(IReadOnlyList<string> Patterns) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        Patterns.Any(p => ctx.NormalizedDescription.Contains(
            p, StringComparison.OrdinalIgnoreCase));
}

public record RegexMatch(Regex Pattern) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        Pattern.IsMatch(ctx.Description);
}

public record AmountCondition(decimal Threshold, ComparisonOp Op) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => Op switch
    {
        ComparisonOp.GreaterThan => ctx.Amount > Threshold,
        ComparisonOp.LessThan => ctx.Amount < Threshold,
        ComparisonOp.GreaterOrEqual => ctx.Amount >= Threshold,
        ComparisonOp.LessOrEqual => ctx.Amount <= Threshold,
        _ => false
    };
}

public record AndExpression(IMatchExpression Left, IMatchExpression Right) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => 
        Left.Matches(ctx) && Right.Matches(ctx);
}

public record OrExpression(IMatchExpression Left, IMatchExpression Right) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => 
        Left.Matches(ctx) || Right.Matches(ctx);
}
```

### 7.3 Rule Application Service

```csharp
// Infrastructure/Services/RulesEngine/RuleApplicationService.cs
public sealed class RuleApplicationService
{
    private readonly FinTrackDbContext _db;
    private readonly RuleParser _parser;
    
    public async Task<RuleApplicationResult> ApplyRulesAsync(
        Guid profileId, 
        IEnumerable<Guid>? transactionIds = null,
        CancellationToken ct = default)
    {
        var profile = await _db.Profiles
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == profileId, ct)
            ?? throw new NotFoundException("Profile not found");
            
        var rulesText = await _db.ProfileRules
            .Where(r => r.ProfileId == profileId)
            .Select(r => r.TomlContent)
            .FirstOrDefaultAsync(ct);
            
        if (string.IsNullOrEmpty(rulesText))
            return RuleApplicationResult.NoRules();
            
        var parsedRules = _parser.Parse(rulesText);
        var categoryLookup = profile.Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        
        var query = _db.Transactions
            .Where(t => t.Account.ProfileId == profileId)
            .Where(t => !t.IsManualOverride);
            
        if (transactionIds is not null)
            query = query.Where(t => transactionIds.Contains(t.Id));
            
        var transactions = await query.ToListAsync(ct);
        
        var matched = 0;
        var unmatched = 0;
        
        foreach (var transaction in transactions)
        {
            var ctx = new TransactionContext(
                transaction.Description,
                transaction.NormalizedDescription,
                Math.Abs(transaction.Amount)
            );
            
            var matchingRule = parsedRules
                .OrderBy(r => r.Priority)
                .FirstOrDefault(r => r.MatchExpression.Matches(ctx));
                
            if (matchingRule is not null && 
                categoryLookup.TryGetValue(matchingRule.Category, out var category))
            {
                transaction.CategoryId = category.Id;
                transaction.MatchedRuleId = /* rule id lookup */;
                transaction.Tags = matchingRule.Tags.ToList();
                matched++;
            }
            else
            {
                unmatched++;
            }
        }
        
        await _db.SaveChangesAsync(ct);
        
        return new RuleApplicationResult(matched, unmatched, transactions.Count);
    }
}
```

---

## 8. LLM Integration

### 8.1 OpenRouter Service

```csharp
// Infrastructure/Services/LlmService/OpenRouterService.cs
public sealed class OpenRouterService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<LlmOptions> _options;
    
    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        var request = new
        {
            model = _options.Value.Model,  // e.g., "openai/gpt-4-turbo"
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.1,
            max_tokens = 2000
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            request,
            ct);
            
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content
            .ReadFromJsonAsync<OpenRouterResponse>(ct);
            
        return result?.Choices?.FirstOrDefault()?.Message?.Content 
            ?? throw new LlmException("Empty response from LLM");
    }
}

// Configuration
public sealed class LlmOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "openai/gpt-4-turbo";
    public string? LocalEndpoint { get; set; }  // For future local model support
}
```

### 8.2 NLQ Prompt Template

```csharp
// Infrastructure/Services/NaturalLanguage/NlqPromptBuilder.cs
public sealed class NlqPromptBuilder
{
    public string BuildSystemPrompt(ProfileSchema schema) => $"""
        You are a SQL query generator for a personal finance application.
        
        DATABASE SCHEMA:
        {schema.ToSql()}
        
        RULES:
        1. Generate ONLY SELECT queries - never INSERT, UPDATE, DELETE, DROP, etc.
        2. Always filter by profile_id = @ProfileId (provided as parameter)
        3. Use PostgreSQL syntax
        4. For date ranges, use proper date comparisons
        5. Return JSON with: {{ "sql": "...", "parameters": {{...}}, "resultType": "scalar|table|error" }}
        6. For aggregations returning single values, use resultType "scalar"
        7. For lists or grouped data, use resultType "table"
        8. If the question cannot be answered with the schema, return resultType "error" with explanation
        
        AVAILABLE TABLES:
        - transactions (id, account_id, date, description, amount, category_id, tags)
        - categories (id, profile_id, name, parent_id)
        - accounts (id, profile_id, name, bank_name)
        
        EXAMPLES:
        Q: "How much did I spend on food last month?"
        A: {{"sql": "SELECT COALESCE(SUM(ABS(amount)), 0) as total FROM transactions t JOIN categories c ON t.category_id = c.id WHERE c.name ILIKE 'Food' AND c.profile_id = @ProfileId AND t.date >= date_trunc('month', CURRENT_DATE - INTERVAL '1 month') AND t.date < date_trunc('month', CURRENT_DATE)", "parameters": {{}}, "resultType": "scalar"}}
        
        Q: "Show me all Netflix transactions"
        A: {{"sql": "SELECT t.date, t.description, t.amount FROM transactions t JOIN accounts a ON t.account_id = a.id WHERE a.profile_id = @ProfileId AND t.description ILIKE '%netflix%' ORDER BY t.date DESC", "parameters": {{}}, "resultType": "table"}}
        """;
        
    public string BuildUserPrompt(string question) => 
        $"Question: {question}";
}
```

### 8.3 Import Format Detection Prompt

```csharp
public sealed class ImportFormatPromptBuilder
{
    public string BuildSystemPrompt() => """
        You are a CSV format analyzer. Given sample rows from a bank statement CSV,
        identify the column mappings.
        
        Respond with JSON:
        {
            "dateColumn": <0-based index>,
            "dateFormat": "<C# date format string>",
            "descriptionColumn": <0-based index>,
            "amountColumn": <0-based index>,
            "creditColumn": <0-based index or null>,
            "debitColumn": <0-based index or null>,
            "hasHeader": <true/false>,
            "delimiter": "<character>",
            "amountSignInverted": <true if positive means expense>,
            "confidence": <0-1>,
            "reasoning": "<brief explanation>"
        }
        
        Common date formats:
        - "dd/MM/yyyy" (European)
        - "MM/dd/yyyy" (US)
        - "yyyy-MM-dd" (ISO)
        - "dd-MM-yyyy"
        
        Look for:
        - Headers with words like "Date", "Data", "Description", "Descrição", "Amount", "Valor"
        - Currency symbols (€, $)
        - Negative numbers for expenses vs separate credit/debit columns
        """;
        
    public string BuildUserPrompt(IReadOnlyList<string[]> sampleRows) =>
        $"Analyze this CSV data:\n\n{FormatSample(sampleRows)}";
}
```

---

## 9. Testing Strategy

### 9.1 Integration Tests (Endpoints)

```csharp
// tests/FinTrack.Tests.Integration/ProfileEndpointsTests.cs
public class ProfileEndpointsTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FinTrackWebApplicationFactory _factory;
    
    public ProfileEndpointsTests(FinTrackWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateProfile_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _client.AuthenticateAsTestUser();
        var request = new { Name = "Business", Type = "Business" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/profiles", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.Name.Should().Be("Business");
    }
    
    [Fact]
    public async Task ImportTransactions_WithValidCsv_CreatesTransactions()
    {
        // Arrange
        await _client.AuthenticateAsTestUser();
        var profileId = await CreateTestProfile();
        var accountId = await CreateTestAccount(profileId);
        
        var csvContent = """
            Date,Description,Amount
            2024-01-15,NETFLIX,9.99
            2024-01-16,UBER TRIP,25.50
            """;
        
        // Act - Upload
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(csvContent), "file", "transactions.csv");
        var uploadResponse = await _client.PostAsync(
            $"/api/profiles/{profileId}/import/upload", content);
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<ImportUploadResult>();
        
        // Act - Analyze
        var analyzeResponse = await _client.PostAsJsonAsync(
            $"/api/profiles/{profileId}/import/analyze",
            new { FileName = uploadResult!.FileName });
        var analyzeResult = await analyzeResponse.Content.ReadFromJsonAsync<ImportAnalysisResult>();
        
        // Act - Confirm
        var confirmResponse = await _client.PostAsJsonAsync(
            $"/api/profiles/{profileId}/import/confirm",
            new { BatchId = analyzeResult!.BatchId, AccountId = accountId });
        
        // Assert
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await confirmResponse.Content.ReadFromJsonAsync<ImportResult>();
        result!.ImportedCount.Should().Be(2);
    }
}

// Test factory
public class FinTrackWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL with in-memory for tests
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FinTrackDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);
                
            services.AddDbContext<FinTrackDbContext>(options =>
                options.UseNpgsql(GetTestConnectionString()));
                
            // Mock LLM service for deterministic tests
            services.AddSingleton<ILlmService, MockLlmService>();
        });
    }
}
```

### 9.2 Unit Tests (Logic)

```csharp
// tests/FinTrack.Tests.Unit/RulesEngine/RuleParserTests.cs
public class RuleParserTests
{
    private readonly RuleParser _parser = new();
    
    [Fact]
    public void Parse_NormalizedMatch_ParsesCorrectly()
    {
        var toml = """
            [netflix]
            match = 'normalized("NETFLIX")'
            category = "Subscriptions"
            """;
            
        var rules = _parser.Parse(toml);
        
        rules.Should().HaveCount(1);
        rules[0].Name.Should().Be("netflix");
        rules[0].MatchExpression.Should().BeOfType<NormalizedMatch>();
    }
    
    [Theory]
    [InlineData("NETFLIX.COM", true)]
    [InlineData("NET FLIX", true)]
    [InlineData("netflix", true)]
    [InlineData("AMAZON", false)]
    public void NormalizedMatch_MatchesNormalizedText(string description, bool expected)
    {
        var match = new NormalizedMatch("NETFLIX");
        var ctx = new TransactionContext(description, Normalize(description), 10m);
        
        match.Matches(ctx).Should().Be(expected);
    }
    
    [Fact]
    public void Parse_ComplexExpression_ParsesCorrectly()
    {
        var toml = """
            [large_amazon]
            match = 'anyof("AMAZON", "AMZN") and amount > 100'
            category = "Shopping"
            """;
            
        var rules = _parser.Parse(toml);
        
        rules[0].MatchExpression.Should().BeOfType<AndExpression>();
    }
}

// tests/FinTrack.Tests.Unit/RulesEngine/RuleMatchingTests.cs
public class RuleMatchingTests
{
    [Theory]
    [InlineData("UBER TRIP TO AIRPORT", true)]
    [InlineData("UBER EATS ORDER", false)]  // Negative lookahead
    [InlineData("UBEREATS", false)]
    public void UberRidesRule_ExcludesUberEats(string description, bool shouldMatch)
    {
        var rule = new RegexMatch(new Regex(@"UBER\s(?!EATS)", RegexOptions.IgnoreCase));
        var ctx = new TransactionContext(description, description.ToUpper(), 25m);
        
        rule.Matches(ctx).Should().Be(shouldMatch);
    }
}
```

### 9.3 Test Categories

| Category | Scope | Tool | Location |
|----------|-------|------|----------|
| Integration | API endpoints, full stack | xUnit + WebApplicationFactory | `Tests.Integration/` |
| Unit | Business logic, parsers | xUnit | `Tests.Unit/` |
| Component | React components | Vitest + Testing Library | `ClientApp/src/**/*.test.tsx` |
| E2E (optional) | Full user flows | Playwright | `Tests.E2E/` |

---

## 10. Deployment

### 10.1 Docker Compose

```yaml
# docker-compose.yml
services:
  app:
    build:
      context: .
      dockerfile: src/FinTrack.Host/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Database=fintrack;Username=fintrack;Password=${DB_PASSWORD}
      - Llm__ApiKey=${OPENROUTER_API_KEY}
      - Llm__Model=openai/gpt-4-turbo
    depends_on:
      db:
        condition: service_healthy
    volumes:
      - app-data:/app/data
    restart: unless-stopped

  db:
    image: postgres:17
    environment:
      - POSTGRES_USER=fintrack
      - POSTGRES_PASSWORD=${DB_PASSWORD}
      - POSTGRES_DB=fintrack
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U fintrack"]
      interval: 5s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres-data:
  app-data:
```

### 10.2 Dockerfile

```dockerfile
# src/FinTrack.Host/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js for React build
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs

# Copy csproj files and restore
COPY ["src/FinTrack.Host/FinTrack.Host.csproj", "src/FinTrack.Host/"]
COPY ["src/FinTrack.Core/FinTrack.Core.csproj", "src/FinTrack.Core/"]
COPY ["src/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj", "src/FinTrack.Infrastructure/"]
RUN dotnet restore "src/FinTrack.Host/FinTrack.Host.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/FinTrack.Host"

# Build React app
WORKDIR "/src/src/FinTrack.Host/ClientApp"
RUN npm ci && npm run build

# Build .NET app
WORKDIR "/src/src/FinTrack.Host"
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create non-root user
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "FinTrack.Host.dll"]
```

### 10.3 Environment Configuration

```bash
# .env.example
DB_PASSWORD=your-secure-password-here
OPENROUTER_API_KEY=sk-or-v1-xxxx

# Optional: Use local model instead
# LLM_LOCAL_ENDPOINT=http://localhost:11434/v1
```

---

## 11. Development Milestones

### Phase 1: Foundation (Week 1-2)
- [ ] Project setup (solution structure, Docker Compose)
- [ ] User authentication (simple, cookie-based)
- [ ] Profile and Account CRUD
- [ ] Basic React shell with routing

### Phase 2: Import & Rules (Week 3-4)
- [ ] CSV upload and parsing
- [ ] LLM format detection
- [ ] Import preview and confirmation
- [ ] TOML rules parser
- [ ] Rule application service

### Phase 3: Dashboard (Week 5-6)
- [ ] Transaction list with filters
- [ ] Dashboard visualizations
- [ ] Category management
- [ ] Manual categorization

### Phase 4: NLQ & Export (Week 7-8)
- [ ] Natural language query interface
- [ ] SQL generation and execution
- [ ] JSON export/import
- [ ] CSV export

### Phase 5: Polish (Week 9-10)
- [ ] Error handling improvements
- [ ] Performance optimization
- [ ] Documentation
- [ ] Testing coverage

---

## 12. Open Questions & Future Considerations

1. **Recurring transactions**: Should we auto-detect and flag recurring expenses?
2. **Budget tracking**: Full budget feature with alerts?
3. **Multi-currency**: Support accounts in different currencies with conversion?
4. **Bank connections**: Direct bank integration via Open Banking APIs?
5. **Mobile**: PWA support for mobile access?
6. **Collaboration**: Share profiles with accountant or partner?

---

## References

- [davidfowl/tally](https://github.com/davidfowl/tally) - Inspiration for rules engine
- [Wolverine FX Documentation](https://wolverinefx.io/)
- [EF Core with PostgreSQL](https://www.npgsql.org/efcore/)
- [OpenRouter API](https://openrouter.ai/docs)
- [Tomlyn (TOML parser)](https://github.com/xoofx/Tomlyn)
