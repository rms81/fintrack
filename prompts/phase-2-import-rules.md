# Phase 2: Import & Rules

## Duration: Week 3-4

## Overview
Implement CSV import with LLM-assisted format detection and the TOML-based rules engine for transaction categorization.

## Goals
1. Create Transaction and Category entities
2. Build CSV upload and parsing
3. Implement LLM format detection via OpenRouter
4. Create import preview and confirmation flow
5. Build TOML rules parser
6. Implement rule matching and application

---

## Task 2.1: Transaction Entity

### Prompt
```
Create the Transaction entity and configuration:

Entity:
- Transaction
  - Id (UUIDv7)
  - AccountId (FK)
  - ExternalId (nullable, from bank)
  - Date (DateOnly)
  - Description (string)
  - NormalizedDescription (uppercase, no special chars)
  - Amount (decimal, negative for expenses)
  - CategoryId (FK, nullable)
  - MatchedRuleId (FK, nullable)
  - Tags (text[] array)
  - IsManualOverride (bool)
  - RawData (jsonb, original CSV row)
  - ImportBatchId (FK, nullable)
  - CreatedAt

Indexes:
- AccountId
- Date
- CategoryId
- Composite for duplicate detection (AccountId, Date, Amount, Description)
- GIN index on NormalizedDescription for fuzzy search
- GIN index on Tags for array containment

Include migration.
```

### Expected Output
- Transaction entity
- EF configuration with all indexes
- Migration

---

## Task 2.2: Category Entity

### Prompt
```
Create the Category entity with hierarchy support:

Entity:
- Category
  - Id (UUIDv7)
  - ProfileId (FK)
  - Name (string)
  - ParentId (FK, nullable, self-reference)
  - Icon (string, nullable)
  - Color (string, hex color)
  - SortOrder (int)
  - CreatedAt

Create default categories seeding:
- Food (Groceries, Restaurants, Delivery)
- Transport (Fuel, Public Transport, Rideshare, Parking)
- Shopping (Clothing, Electronics, Home, Other)
- Housing (Rent, Utilities, Insurance, Maintenance)
- Entertainment (Streaming, Games, Events, Hobbies)
- Health (Medical, Pharmacy, Gym)
- Subscriptions (Software, Services, Memberships)
- Travel (Flights, Hotels, Car Rental)
- Income (Salary, Freelance, Investments, Other)
- Transfer (Internal transfers)
- Other (Uncategorized)

Include endpoints:
- GET /api/profiles/{profileId}/categories
- POST /api/profiles/{profileId}/categories
- PUT /api/profiles/{profileId}/categories/{id}
- DELETE /api/profiles/{profileId}/categories/{id}
```

### Expected Output
- Category entity and configuration
- Seeding logic
- CRUD endpoints

---

## Task 2.3: ImportFormat Entity

### Prompt
```
Create the ImportFormat entity for saved CSV mappings:

Entity:
- ImportFormat
  - Id (UUIDv7)
  - ProfileId (FK)
  - Name (string)
  - BankName (string, nullable)
  - Mapping (jsonb)
  - CreatedAt

Mapping Value Object:
- DateColumn (int)
- DateFormat (string, C# format)
- DescriptionColumn (int)
- AmountColumn (int)
- CreditColumn (int?, for split debit/credit)
- DebitColumn (int?)
- HasHeader (bool)
- Delimiter (char)
- Encoding (string?, e.g., "UTF-8")
- AmountSignInverted (bool)

Endpoints:
- GET /api/profiles/{profileId}/import-formats
- POST /api/profiles/{profileId}/import-formats
- DELETE /api/profiles/{profileId}/import-formats/{id}
```

### Expected Output
- ImportFormat entity
- Mapping value object
- CRUD endpoints

---

## Task 2.4: CSV Upload

### Prompt
```
Implement CSV file upload:

1. Create upload endpoint:
   POST /api/profiles/{profileId}/import/upload
   - Accept multipart/form-data
   - Validate file size (max 10MB)
   - Validate file extension (.csv)
   - Store temporarily in /tmp or configured path
   - Return upload result with filename and preview rows

2. Create ImportBatch entity to track imports:
   - Id, ProfileId, AccountId, FileName, OriginalFileName
   - Status (Pending, Analyzing, AwaitingConfirmation, Processing, Completed, Failed)
   - TotalRows, ImportedCount, DuplicateCount, ErrorCount
   - ErrorDetails (text)
   - CreatedAt, CompletedAt

3. Parse first 20 rows for preview
   - Auto-detect delimiter (comma, semicolon, tab)
   - Detect if has header row
   - Return as JSON array
```

### Expected Output
- Upload endpoint
- ImportBatch entity
- CSV parsing service

---

## Task 2.5: LLM Format Detection

### Prompt
```
Implement LLM-assisted CSV format detection using OpenRouter:

1. Create OpenRouter service:
   - Configure from appsettings (ApiKey, Model, BaseUrl)
   - Support chat completion API
   - Handle rate limiting and errors

2. Create format detection prompt:
   - Send first 10-15 rows to LLM
   - Ask to identify column mappings
   - Return structured JSON response
   - Parse and validate response

3. Create analyze endpoint:
   POST /api/profiles/{profileId}/import/analyze
   - Input: { fileName, formatId? }
   - If formatId provided, use saved format
   - Otherwise, call LLM for detection
   - Return detected mapping with confidence score
   - Allow user to override mappings

4. Store analysis result in ImportBatch

Prompt template should:
- Include common bank formats (European date, currency symbols)
- Handle both single amount column and split debit/credit
- Detect sign convention (positive = expense or income)
```

### Expected Output
- OpenRouter service
- Format detection prompt
- Analyze endpoint
- Response parsing

---

## Task 2.6: Import Confirmation

### Prompt
```
Implement import confirmation and transaction creation:

1. Create confirm endpoint:
   POST /api/profiles/{profileId}/import/confirm
   - Input: { batchId, accountId, mapping, saveFormat?, formatName? }
   - Parse CSV with confirmed mapping
   - Create transactions
   - Handle duplicates (by ExternalId or Date+Amount+Description)
   - Apply rules after import
   - Update ImportBatch status

2. Duplicate detection:
   - Check ExternalId if present
   - Otherwise check Date + Amount + normalized Description
   - Mark as duplicate, don't create

3. Transaction creation:
   - Generate UUIDv7 for Id
   - Normalize description (uppercase, remove special chars)
   - Store raw CSV row as JSONB
   - Link to ImportBatch

4. Return result:
   - { importedCount, duplicateCount, errorCount, errors[] }
```

### Expected Output
- Confirm endpoint
- Duplicate detection logic
- Transaction creation

---

## Task 2.7: Rule Entity

### Prompt
```
Create the Rule entity for storing parsed rules:

Entity:
- Rule
  - Id (UUIDv7)
  - ProfileId (FK)
  - Name (string, unique per profile)
  - Priority (int, lower = higher priority)
  - MatchExpression (string, the TOML match value)
  - CategoryId (FK)
  - Subcategory (string, nullable)
  - Tags (text[])
  - IsEnabled (bool)
  - CreatedAt

Also create ProfileRules entity for raw TOML storage:
- Id, ProfileId, TomlContent, UpdatedAt

Endpoints:
- GET /api/profiles/{profileId}/rules/toml - Get raw TOML
- PUT /api/profiles/{profileId}/rules/toml - Update TOML (parse and save)
- POST /api/profiles/{profileId}/rules/validate - Validate TOML syntax
```

### Expected Output
- Rule entity
- ProfileRules entity
- TOML endpoints

---

## Task 2.8: TOML Parser

### Prompt
```
Implement the TOML rules parser using Tomlyn:

1. Create RuleParser service:
   - Parse TOML using Tomlyn
   - Validate required fields (match, category)
   - Return list of ParsedRule objects

2. Create expression parser for match expressions:
   - Tokenize expression string
   - Parse into expression tree
   - Support: normalized(), contains(), startswith(), anyof(), regex()
   - Support: amount comparisons (>, <, >=, <=)
   - Support: and, or operators with precedence

3. Create IMatchExpression interface and implementations:
   - NormalizedMatch
   - ContainsMatch
   - StartsWithMatch
   - AnyOfMatch
   - RegexMatch
   - AmountCondition
   - AndExpression
   - OrExpression

4. Include comprehensive unit tests for parser
```

### Expected Output
- Tomlyn integration
- Expression parser
- Match expression types
- Unit tests

---

## Task 2.9: Rule Application

### Prompt
```
Implement rule application service:

1. Create RuleApplicationService:
   - Load rules for profile
   - Parse expressions
   - Apply to transactions
   - First matching rule wins (by priority)
   - Preserve manual overrides

2. Create apply endpoint:
   POST /api/profiles/{profileId}/rules/apply
   - Input: { transactionIds? } (optional, for specific transactions)
   - Apply rules to all non-manual transactions if no ids
   - Return { matchedCount, unmatchedCount, totalCount }

3. Create test endpoint:
   POST /api/profiles/{profileId}/rules/test
   - Input: { rule, sampleTransactions[] }
   - Test a single rule against sample data
   - Return match results

4. Auto-apply rules after import (in confirm endpoint)
```

### Expected Output
- RuleApplicationService
- Apply and test endpoints
- Integration with import

---

## Task 2.10: Import UI

### Prompt
```
Create the Import UI flow:

Pages:
- /import - Main import page

Components:
1. FileUpload
   - Drag and drop zone
   - File validation
   - Upload progress

2. FormatSelector
   - Show saved formats
   - Option to detect new format

3. MappingEditor
   - Show CSV preview table
   - Column dropdowns for mapping
   - Date format selector
   - Amount sign toggle

4. ImportPreview
   - Show parsed transactions preview
   - Highlight potential duplicates
   - Show validation errors

5. ImportProgress
   - Progress bar during import
   - Show results summary

Flow:
1. Upload CSV → 2. Select/Detect Format → 3. Review Mapping → 
4. Preview Transactions → 5. Confirm Import → 6. Show Results
```

### Expected Output
- Import wizard components
- Multi-step form flow
- Preview and confirmation UI

---

## Task 2.11: Rules Editor UI

### Prompt
```
Create the Rules Editor UI:

Page: /rules

Components:
1. RulesEditor
   - Monaco editor with TOML syntax highlighting
   - Save button
   - Validation status indicator

2. RulesSidebar
   - List of rules (from parsed TOML)
   - Click to jump to rule in editor
   - Show category and match preview

3. RuleTester
   - Input sample transaction description and amount
   - Show which rule would match
   - Show assigned category and tags

4. RulesDocumentation
   - Syntax reference
   - Example rules
   - Available functions

Features:
- Auto-save with debounce
- Validation on change
- Error highlighting in editor
```

### Expected Output
- Monaco editor integration
- Rules management UI
- Rule testing interface

---

## Completion Criteria

Phase 2 is complete when:
- [ ] Categories with hierarchy work
- [ ] CSV upload accepts files
- [ ] LLM detects format correctly
- [ ] Import formats can be saved
- [ ] Transactions are created from CSV
- [ ] Duplicate detection works
- [ ] TOML rules parse correctly
- [ ] Rules apply to transactions
- [ ] Import UI wizard completes
- [ ] Rules editor with Monaco works
- [ ] All tests pass

## Next Phase
Proceed to Phase 3: Dashboard
