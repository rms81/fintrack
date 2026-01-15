#!/bin/bash

# Script to create GitHub issues for FinTrack project
# This script creates issues for all incomplete user stories and tasks

set -e

echo "Creating GitHub issues for FinTrack project..."
echo ""

# =============================================================================
# IMPORT MODULE USER STORIES (Phase 2)
# =============================================================================

echo "Creating Import module user stories..."

gh issue create \
  --title "[US-I1] Upload CSV file from bank" \
  --body "**As a** User
**I want to** upload a CSV file from my bank
**So that** I can import my transactions

## Acceptance Criteria
- [ ] User can select and upload a CSV file
- [ ] File validation (size, extension)
- [ ] File is temporarily stored for processing
- [ ] Preview of first rows is shown

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.4 (CSV Upload)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I2] Auto-detect CSV format" \
  --body "**As a** User
**I want to** have the format auto-detected
**So that** I don't need to manually configure column mappings

## Acceptance Criteria
- [ ] LLM analyzes CSV sample
- [ ] Column mappings are detected (date, description, amount)
- [ ] Date format is identified
- [ ] Delimiter and encoding are detected

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.5 (LLM Format Detection)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I3] Preview transactions before importing" \
  --body "**As a** User
**I want to** preview transactions before importing
**So that** I can verify the data looks correct

## Acceptance Criteria
- [ ] Parsed transactions are displayed in preview table
- [ ] All columns are shown with correct formatting
- [ ] User can see potential issues before confirming
- [ ] Navigation between preview and confirmation

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.6 (Import Confirmation)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I4] Adjust format settings if auto-detection is wrong" \
  --body "**As a** User
**I want to** adjust format settings if auto-detection is wrong
**So that** I can handle unusual CSV formats

## Acceptance Criteria
- [ ] User can manually override detected column mappings
- [ ] Date format selector is available
- [ ] Amount sign convention can be toggled
- [ ] Delimiter can be changed
- [ ] Changes reflect immediately in preview

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.10 (Import UI)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I5] Warn about duplicate transactions" \
  --body "**As a** User
**I want to** be warned about duplicate transactions
**So that** I don't accidentally import the same data twice

## Acceptance Criteria
- [ ] Duplicate detection runs during import
- [ ] Duplicates are highlighted in preview
- [ ] User is informed of duplicate count
- [ ] Duplicates are skipped during import

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.6 (Import Confirmation)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I6] See import progress and status" \
  --body "**As a** User
**I want to** see import progress and status
**So that** I know when the import is complete

## Acceptance Criteria
- [ ] Progress bar shows during import
- [ ] Status updates are shown (processing, applying rules, etc.)
- [ ] Final results show imported/duplicate/error counts
- [ ] User can navigate to view imported transactions

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.10 (Import UI)" \
  --label "user-story,phase-2,import"

gh issue create \
  --title "[US-I7] Have rules automatically applied during import" \
  --body "**As a** User
**I want to** have rules automatically applied during import
**So that** new transactions are categorized immediately

## Acceptance Criteria
- [ ] Rules are applied after transactions are created
- [ ] Matched rules are recorded on transactions
- [ ] Import results show categorization statistics
- [ ] Manual override flag is not set for rule-based categorization

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.9 (Rule Application)" \
  --label "user-story,phase-2,import,rules-engine"

gh issue create \
  --title "[US-I8] View import history" \
  --body "**As a** User
**I want to** view my import history
**So that** I can track which files I've already imported

## Acceptance Criteria
- [ ] List of past imports is displayed
- [ ] Each import shows filename, date, and statistics
- [ ] User can see which account each import was for
- [ ] Failed imports are clearly marked

## Related
- Module: Import (.claude/modules/import.md)
- Phase: 2 (Import & Rules)
- Task: 2.4 (CSV Upload)" \
  --label "user-story,phase-2,import"

# =============================================================================
# RULES ENGINE MODULE USER STORIES (Phase 2)
# =============================================================================

echo "Creating Rules Engine module user stories..."

gh issue create \
  --title "[US-R1] Create categorization rules" \
  --body "**As a** User
**I want to** create categorization rules
**So that** transactions are automatically categorized

## Acceptance Criteria
- [ ] User can create new rules via TOML editor
- [ ] Rules are validated on save
- [ ] Rules are stored per profile
- [ ] Rules can be enabled/disabled

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.7 (Rule Entity)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R2] Match rules by description text" \
  --body "**As a** User
**I want to** match rules by description text
**So that** I can target specific merchants or keywords

## Acceptance Criteria
- [ ] Support contains() matcher
- [ ] Support equals() matcher
- [ ] Support regex() matcher
- [ ] Case-insensitive matching available

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.8 (TOML Parser)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R3] Match rules by amount ranges" \
  --body "**As a** User
**I want to** match rules by amount ranges
**So that** I can categorize based on transaction size

## Acceptance Criteria
- [ ] Support amount equals
- [ ] Support amount greater than
- [ ] Support amount less than
- [ ] Support amount range [min, max]

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.8 (TOML Parser)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R4] Set rule priority" \
  --body "**As a** User
**I want to** set rule priority
**So that** I can control which rule applies when multiple match

## Acceptance Criteria
- [ ] Rules have priority field (lower = higher priority)
- [ ] First matching rule wins
- [ ] Priority is respected during application
- [ ] Rules can be reordered

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.9 (Rule Application)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R5] Test rules against sample transactions" \
  --body "**As a** User
**I want to** test rules against sample transactions
**So that** I can verify rules work before saving

## Acceptance Criteria
- [ ] Test endpoint accepts rule and sample data
- [ ] Shows which transactions would match
- [ ] Shows assigned category and tags
- [ ] Highlights matching conditions

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.9 (Rule Application)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R6] Apply rules to existing uncategorized transactions" \
  --body "**As a** User
**I want to** apply rules to existing uncategorized transactions
**So that** historical data gets categorized

## Acceptance Criteria
- [ ] Bulk apply endpoint available
- [ ] Only applies to uncategorized transactions (or all if requested)
- [ ] Respects manual override flag
- [ ] Returns statistics on matches

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.9 (Rule Application)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R7] Enable/disable rules without deleting" \
  --body "**As a** User
**I want to** enable/disable rules without deleting
**So that** I can temporarily turn off rules

## Acceptance Criteria
- [ ] Rules have IsEnabled flag
- [ ] Disabled rules are skipped during application
- [ ] UI shows enabled/disabled status
- [ ] Toggle is easy to access

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.7 (Rule Entity)" \
  --label "user-story,phase-2,rules-engine"

gh issue create \
  --title "[US-R8] Use rule templates" \
  --body "**As a** User
**I want to** use rule templates
**So that** I can quickly add common categorization patterns

## Acceptance Criteria
- [ ] Template library is available
- [ ] Templates cover common scenarios (Uber, Netflix, groceries, etc.)
- [ ] User can insert template into editor
- [ ] Templates can be customized

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.11 (Rules Editor UI)" \
  --label "user-story,phase-2,rules-engine,enhancement"

gh issue create \
  --title "[US-R9] Add tags via rules" \
  --body "**As a** User
**I want to** add tags via rules
**So that** I can automatically tag transactions for grouping

## Acceptance Criteria
- [ ] Rules can specify tags array
- [ ] Tags are applied along with category
- [ ] Multiple tags can be added
- [ ] Tags can be used for filtering

## Related
- Module: Rules Engine (.claude/modules/rules-engine.md)
- Phase: 2 (Import & Rules)
- Task: 2.8 (TOML Parser)" \
  --label "user-story,phase-2,rules-engine"

# =============================================================================
# TRANSACTIONS MODULE USER STORIES (Phase 2/3)
# =============================================================================

echo "Creating Transactions module user stories..."

gh issue create \
  --title "[US-T1] View all transactions in a list" \
  --body "**As a** User
**I want to** view all my transactions in a list
**So that** I can see my financial activity

## Acceptance Criteria
- [ ] Transactions are displayed in a table/list
- [ ] Pagination or infinite scroll works
- [ ] Columns show: Date, Description, Category, Amount
- [ ] Performance is good with many transactions

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T2] Filter transactions by date range" \
  --body "**As a** User
**I want to** filter transactions by date range
**So that** I can focus on a specific time period

## Acceptance Criteria
- [ ] Date range picker is available
- [ ] Presets provided (This Month, Last Month, etc.)
- [ ] Custom range selection works
- [ ] Filter updates transaction list

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T3] Filter transactions by category" \
  --body "**As a** User
**I want to** filter transactions by category
**So that** I can see spending in specific areas

## Acceptance Criteria
- [ ] Category filter is available
- [ ] Multi-select categories supported
- [ ] Uncategorized filter option available
- [ ] Filter persists in URL

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T4] Filter transactions by account" \
  --body "**As a** User
**I want to** filter transactions by account
**So that** I can reconcile with bank statements

## Acceptance Criteria
- [ ] Account filter is available
- [ ] Multi-select accounts supported
- [ ] Shows account name in transaction list
- [ ] Filter persists in URL

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T5] Search transactions by description" \
  --body "**As a** User
**I want to** search transactions by description
**So that** I can find specific purchases

## Acceptance Criteria
- [ ] Search input is available
- [ ] Fuzzy/full-text search works
- [ ] Search is debounced
- [ ] Highlights matching text

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T6] Edit a transaction's category" \
  --body "**As a** User
**I want to** edit a transaction's category
**So that** I can correct miscategorized items

## Acceptance Criteria
- [ ] Category can be changed via dropdown
- [ ] Change is saved immediately (or on confirmation)
- [ ] Manual override flag is set
- [ ] Change reflects in dashboard

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.2 (Transaction Detail)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T7] Add notes to a transaction" \
  --body "**As a** User
**I want to** add notes to a transaction
**So that** I can remember context about a purchase

## Acceptance Criteria
- [ ] Notes field is available in detail view
- [ ] Notes are saved
- [ ] Notes are displayed in transaction list (optional)
- [ ] Notes are searchable

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.2 (Transaction Detail)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T8] Add tags to transactions" \
  --body "**As a** User
**I want to** add tags to transactions
**So that** I can create custom groupings across categories

## Acceptance Criteria
- [ ] Tags can be added/removed
- [ ] Tag autocomplete available
- [ ] Tags are filterable
- [ ] Tags are displayed as badges

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.2 (Transaction Detail)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T9] Delete a transaction" \
  --body "**As a** User
**I want to** delete a transaction
**So that** I can remove duplicates or errors

## Acceptance Criteria
- [ ] Delete action is available
- [ ] Confirmation dialog shown
- [ ] Transaction is removed from database
- [ ] Dashboard updates accordingly

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.3 (Bulk Operations)" \
  --label "user-story,phase-3,transactions"

gh issue create \
  --title "[US-T10] See uncategorized transactions" \
  --body "**As a** User
**I want to** see uncategorized transactions
**So that** I can ensure all transactions are properly organized

## Acceptance Criteria
- [ ] Uncategorized filter is available
- [ ] Count of uncategorized shown in dashboard
- [ ] Easy to categorize from this view
- [ ] Bulk categorization available

## Related
- Module: Transactions (.claude/modules/transactions.md)
- Phase: 3 (Dashboard)
- Task: 3.1 (Transaction List Query)" \
  --label "user-story,phase-3,transactions"

# =============================================================================
# DASHBOARD MODULE USER STORIES (Phase 3)
# =============================================================================

echo "Creating Dashboard module user stories..."

gh issue create \
  --title "[US-D1] See spending breakdown by category" \
  --body "**As a** User
**I want to** see spending breakdown by category
**So that** I understand where my money goes

## Acceptance Criteria
- [ ] Pie/donut chart shows category breakdown
- [ ] Percentages are displayed
- [ ] Only expenses (not income) included
- [ ] Drill-down to category details available

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.5 (Spending by Category Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D2] View monthly income vs expenses trend" \
  --body "**As a** User
**I want to** view monthly income vs expenses trend
**So that** I can track my financial health over time

## Acceptance Criteria
- [ ] Line/area chart shows trend over time
- [ ] Income and expenses shown separately
- [ ] Net balance calculated
- [ ] 12-month rolling view

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.6 (Spending Over Time Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D3] See daily spending for current month" \
  --body "**As a** User
**I want to** see daily spending for the current month
**So that** I can monitor day-to-day spending patterns

## Acceptance Criteria
- [ ] Daily breakdown chart available
- [ ] Current month is shown
- [ ] Running balance line available
- [ ] Unusual days are highlighted

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.6 (Spending Over Time Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D4] View top merchants by spend" \
  --body "**As a** User
**I want to** view my top merchants by spend
**So that** I know which businesses I frequent most

## Acceptance Criteria
- [ ] Top 10 merchants shown
- [ ] Total spent per merchant displayed
- [ ] Transaction count shown
- [ ] Click to filter by merchant

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.7 (Top Merchants Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D5] See account balances at a glance" \
  --body "**As a** User
**I want to** see account balances at a glance
**So that** I have a quick overview of my finances

## Acceptance Criteria
- [ ] Summary cards show key metrics
- [ ] Total income shown
- [ ] Total expenses shown
- [ ] Net balance calculated
- [ ] Period comparison available

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.4 (Dashboard Summary Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D6] Compare spending to previous periods" \
  --body "**As a** User
**I want to** compare spending to previous periods
**So that** I can identify changes in habits

## Acceptance Criteria
- [ ] Previous period comparison shown
- [ ] Percentage change calculated
- [ ] Trend indicators (up/down arrows)
- [ ] Same duration comparison (month-to-month, etc.)

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.4 (Dashboard Summary Query)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D7] Drill down into category details" \
  --body "**As a** User
**I want to** drill down into category details
**So that** I can explore specific spending areas

## Acceptance Criteria
- [ ] Click on category navigates to filtered transactions
- [ ] Subcategories shown in breakdown
- [ ] Trend for specific category available
- [ ] Easy navigation back to dashboard

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.9 (Dashboard UI)" \
  --label "user-story,phase-3,dashboard"

gh issue create \
  --title "[US-D8] See count of uncategorized transactions" \
  --body "**As a** User
**I want to** see count of uncategorized transactions
**So that** I know how much cleanup work remains

## Acceptance Criteria
- [ ] Count displayed prominently on dashboard
- [ ] Click to view uncategorized transactions
- [ ] Percentage of total shown
- [ ] Call-to-action if count is high

## Related
- Module: Dashboard (.claude/modules/dashboard.md)
- Phase: 3 (Dashboard)
- Task: 3.4 (Dashboard Summary Query)" \
  --label "user-story,phase-3,dashboard"

# =============================================================================
# NLQ MODULE USER STORIES (Phase 4)
# =============================================================================

echo "Creating NLQ module user stories..."

gh issue create \
  --title "[US-N1] Ask questions in plain English" \
  --body "**As a** User
**I want to** ask questions in plain English
**So that** I can get insights without learning query syntax

## Acceptance Criteria
- [ ] Natural language input available
- [ ] Questions are translated to SQL
- [ ] Results are returned and formatted
- [ ] Error handling for ambiguous questions

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.1 (NLQ Prompt Engineering)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N2] See the generated SQL query" \
  --body "**As a** User
**I want to** see the generated SQL query
**So that** I can understand how my question was interpreted

## Acceptance Criteria
- [ ] SQL is shown in collapsible section
- [ ] Syntax highlighting applied
- [ ] Copy to clipboard available
- [ ] Explanation of query provided

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.3 (NLQ Execution)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N3] View results in table or chart" \
  --body "**As a** User
**I want to** view results in a table or chart
**So that** I can easily understand the data

## Acceptance Criteria
- [ ] Results auto-detect appropriate visualization
- [ ] Table format for list results
- [ ] Chart format for aggregations
- [ ] Scalar results shown prominently

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.4 (Results Rendering)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N4] Re-run previous queries" \
  --body "**As a** User
**I want to** re-run previous queries
**So that** I can quickly access frequent questions

## Acceptance Criteria
- [ ] Query history is stored
- [ ] Past queries are listed
- [ ] Click to re-run
- [ ] Clear history available

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.5 (NLQ UI)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N5] Get query suggestions" \
  --body "**As a** User
**I want to** get query suggestions
**So that** I can discover what questions I can ask

## Acceptance Criteria
- [ ] Example questions are provided
- [ ] Suggestions based on context
- [ ] Click to populate input
- [ ] Follow-up suggestions after results

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.5 (NLQ UI)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N6] Refine or modify my question" \
  --body "**As a** User
**I want to** refine or modify my question
**So that** I can get more specific answers

## Acceptance Criteria
- [ ] Conversation context maintained
- [ ] Follow-up questions work
- [ ] Previous results referenced
- [ ] Easy to edit and re-submit

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.3 (NLQ Execution)" \
  --label "user-story,phase-4,nlq"

gh issue create \
  --title "[US-N7] Export query results" \
  --body "**As a** User
**I want to** export query results
**So that** I can use the data in other tools

## Acceptance Criteria
- [ ] Export to CSV available
- [ ] Export to JSON available
- [ ] File download works
- [ ] Proper formatting for external tools

## Related
- Module: NLQ (.claude/modules/nlq.md)
- Phase: 4 (NLQ & Export)
- Task: 4.8 (CSV Export)" \
  --label "user-story,phase-4,nlq,export"

# =============================================================================
# PHASE 2 TASKS (Import & Rules)
# =============================================================================

echo "Creating Phase 2 task issues..."

gh issue create \
  --title "[Task 2.1] Transaction Entity" \
  --body "## Description
Create the Transaction entity and database configuration with all required fields and indexes.

## Requirements
- Transaction entity with UUIDv7 primary key
- Foreign keys to Account and Category
- Support for tags (text array)
- Raw CSV data storage (jsonb)
- Duplicate detection indexes
- Full-text search on description

## Deliverables
- [ ] Transaction.cs entity
- [ ] TransactionConfiguration.cs (EF Core)
- [ ] Database migration
- [ ] Indexes for performance

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.1)" \
  --label "task,phase-2,backend,database"

gh issue create \
  --title "[Task 2.2] Category Entity" \
  --body "##Description
Create the Category entity with hierarchy support and default category seeding.

## Requirements
- Category entity with parent-child relationship
- Icon and color support
- Profile-scoped categories
- Default categories created on profile creation
- CRUD endpoints

## Deliverables
- [ ] Category.cs entity
- [ ] CategoryConfiguration.cs
- [ ] Default categories seeding logic
- [ ] CRUD endpoints with Wolverine handlers

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.2)" \
  --label "task,phase-2,backend,database"

gh issue create \
  --title "[Task 2.3] ImportFormat Entity" \
  --body "## Description
Create the ImportFormat entity for saved CSV mappings.

## Requirements
- ImportFormat entity for storing CSV configurations
- Mapping value object with column definitions
- Support for various CSV formats
- CRUD endpoints

## Deliverables
- [ ] ImportFormat.cs entity
- [ ] CsvMapping value object
- [ ] CRUD endpoints
- [ ] Database migration

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.3)" \
  --label "task,phase-2,backend,database"

gh issue create \
  --title "[Task 2.4] CSV Upload" \
  --body "## Description
Implement CSV file upload with validation and preview.

## Requirements
- File upload endpoint (multipart/form-data)
- File validation (size, extension)
- ImportBatch entity for tracking
- CSV parsing with preview (first 20 rows)
- Auto-detect delimiter and header

## Deliverables
- [ ] Upload endpoint
- [ ] ImportBatch entity
- [ ] CsvParser service
- [ ] Preview functionality

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.4)" \
  --label "task,phase-2,backend"

gh issue create \
  --title "[Task 2.5] LLM Format Detection" \
  --body "## Description
Implement LLM-assisted CSV format detection using OpenRouter.

## Requirements
- OpenRouter service integration
- Format detection prompt template
- Parse LLM response to mapping configuration
- Confidence scoring
- Analyze endpoint

## Deliverables
- [ ] OpenRouterService
- [ ] Format detection prompt
- [ ] Response parser
- [ ] Analyze endpoint

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.5)
- Integration: integrations/openrouter.md" \
  --label "task,phase-2,backend,llm"

gh issue create \
  --title "[Task 2.6] Import Confirmation" \
  --body "## Description
Implement import confirmation and transaction creation with duplicate detection.

## Requirements
- Confirm endpoint
- Parse CSV with confirmed mapping
- Duplicate detection (ExternalId or hash-based)
- Transaction creation in bulk
- Apply rules after import
- Update ImportBatch status

## Deliverables
- [ ] Confirm endpoint
- [ ] Duplicate detection logic
- [ ] Bulk transaction creation
- [ ] ImportResult DTO

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.6)" \
  --label "task,phase-2,backend"

gh issue create \
  --title "[Task 2.7] Rule Entity" \
  --body "## Description
Create the Rule entity for storing parsed TOML rules.

## Requirements
- Rule entity with match expressions
- ProfileRules entity for raw TOML storage
- Priority-based ordering
- Enable/disable functionality
- TOML endpoints (get, update, validate)

## Deliverables
- [ ] Rule.cs entity
- [ ] ProfileRules.cs entity
- [ ] TOML CRUD endpoints
- [ ] Validation endpoint

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.7)" \
  --label "task,phase-2,backend,database"

gh issue create \
  --title "[Task 2.8] TOML Parser" \
  --body "## Description
Implement the TOML rules parser using Tomlyn library.

## Requirements
- Tomlyn integration
- Expression parser for match conditions
- Support for: contains, startsWith, regex, amount comparisons
- Boolean operators (and, or)
- Comprehensive unit tests

## Deliverables
- [ ] RuleParser service
- [ ] Expression tokenizer
- [ ] IMatchExpression implementations
- [ ] Unit tests (>90% coverage)

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.8)" \
  --label "task,phase-2,backend"

gh issue create \
  --title "[Task 2.9] Rule Application" \
  --body "## Description
Implement rule application service for automatic categorization.

## Requirements
- RuleApplicationService
- Load and parse rules for profile
- Apply rules to transactions (priority-based)
- Preserve manual overrides
- Apply and test endpoints

## Deliverables
- [ ] RuleApplicationService
- [ ] Apply rules endpoint
- [ ] Test rules endpoint
- [ ] Integration with import flow

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.9)" \
  --label "task,phase-2,backend"

gh issue create \
  --title "[Task 2.10] Import UI" \
  --body "## Description
Create the Import UI wizard with all steps from upload to completion.

## Requirements
- Multi-step wizard flow
- FileUpload component (drag & drop)
- FormatSelector component
- MappingEditor with preview table
- ImportPreview with duplicate highlighting
- ImportProgress indicator

## Deliverables
- [ ] Import wizard page (/import)
- [ ] FileUpload component
- [ ] MappingEditor component
- [ ] ImportPreview component
- [ ] ImportProgress component

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.10)" \
  --label "task,phase-2,frontend"

gh issue create \
  --title "[Task 2.11] Rules Editor UI" \
  --body "## Description
Create the Rules Editor UI with Monaco editor and TOML syntax highlighting.

## Requirements
- Monaco editor integration
- TOML syntax highlighting
- Validation and error display
- Rule tester interface
- Rules sidebar with navigation
- Documentation panel

## Deliverables
- [ ] Rules editor page (/rules)
- [ ] Monaco editor integration
- [ ] RulesSidebar component
- [ ] RuleTester component
- [ ] Auto-save with debounce

## Related
- Phase: 2 (Import & Rules)
- Prompt: prompts/phase-2-import-rules.md (Task 2.11)" \
  --label "task,phase-2,frontend"

# =============================================================================
# PHASE 3 TASKS (Dashboard)
# =============================================================================

echo "Creating Phase 3 task issues..."

gh issue create \
  --title "[Task 3.1] Transaction List Query" \
  --body "## Description
Create transaction list query with comprehensive filtering and pagination.

## Requirements
- GetTransactions query with Wolverine handler
- Support for all filters (date, account, category, search, tags)
- Efficient pagination
- PostgreSQL full-text search
- Sorting options

## Deliverables
- [ ] GetTransactions query and handler
- [ ] TransactionDto
- [ ] GET /api/profiles/{profileId}/transactions endpoint
- [ ] Integration tests

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.1)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.2] Transaction Detail" \
  --body "## Description
Create transaction detail view and update functionality.

## Requirements
- GetTransaction query
- UpdateTransaction command
- Manual override tracking
- Create rule from transaction feature
- Category and tags editing

## Deliverables
- [ ] GetTransaction query
- [ ] UpdateTransaction command
- [ ] Detail and update endpoints
- [ ] Create rule endpoint

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.2)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.3] Bulk Operations" \
  --body "## Description
Implement bulk transaction operations (categorize, delete).

## Requirements
- BulkCategorize command
- BulkDelete command
- Authorization checks
- Efficient batch processing

## Deliverables
- [ ] BulkCategorize command and handler
- [ ] BulkDelete command and handler
- [ ] Bulk operation endpoints
- [ ] Integration tests

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.3)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.4] Dashboard Summary Query" \
  --body "## Description
Create dashboard summary query with period comparison.

## Requirements
- GetDashboardSummary query
- Calculate income, expenses, net balance
- Period-over-period comparison
- Transaction count and top category

## Deliverables
- [ ] GetDashboardSummary query
- [ ] DashboardSummaryDto
- [ ] Summary endpoint
- [ ] Period comparison logic

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.4)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.5] Spending by Category Query" \
  --body "## Description
Create spending by category aggregation with hierarchy support.

## Requirements
- GetSpendingByCategory query
- Aggregate expenses by category
- Calculate percentages
- Support subcategories
- Include transaction counts

## Deliverables
- [ ] GetSpendingByCategory query
- [ ] CategorySpendingDto
- [ ] Category aggregation endpoint
- [ ] Subcategory grouping

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.5)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.6] Spending Over Time Query" \
  --body "## Description
Create time series aggregation for spending trends.

## Requirements
- GetSpendingOverTime query
- Support multiple granularities (day, week, month)
- Group by period
- Fill gaps for empty periods
- Separate income and expenses

## Deliverables
- [ ] GetSpendingOverTime query
- [ ] TimeSeriesDto
- [ ] Time series endpoint
- [ ] Gap filling logic

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.6)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.7] Top Merchants Query" \
  --body "## Description
Create top merchants aggregation query.

## Requirements
- GetTopMerchants query
- Group by normalized description
- Calculate totals and counts
- Identify most common category
- Configurable limit

## Deliverables
- [ ] GetTopMerchants query
- [ ] MerchantSpendingDto
- [ ] Top merchants endpoint
- [ ] Category detection logic

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.7)" \
  --label "task,phase-3,backend"

gh issue create \
  --title "[Task 3.8] Transaction List UI" \
  --body "## Description
Create the transaction list UI with filters and detail view.

## Requirements
- Transaction list page (/transactions)
- TransactionFilters component
- TransactionTable with sorting
- TransactionDetail side panel
- BulkActions toolbar
- URL state management

## Deliverables
- [ ] Transaction list page
- [ ] Filter components
- [ ] Table with sorting
- [ ] Detail panel
- [ ] Bulk selection UI

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.8)" \
  --label "task,phase-3,frontend"

gh issue create \
  --title "[Task 3.9] Dashboard UI" \
  --body "## Description
Create the main dashboard UI with charts and summary cards.

## Requirements
- Dashboard page (/ home)
- SummaryCards (4 metrics)
- SpendingByCategory pie chart (Recharts)
- SpendingOverTime area chart
- TopMerchants bar chart
- RecentTransactions list
- Date range picker integration

## Deliverables
- [ ] Dashboard page
- [ ] Summary cards
- [ ] All chart components
- [ ] Recent transactions widget
- [ ] Responsive grid layout

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.9)" \
  --label "task,phase-3,frontend"

gh issue create \
  --title "[Task 3.10] Category Management UI" \
  --body "## Description
Create category management UI with tree view and editing.

## Requirements
- Category management page (/categories)
- CategoryTree with hierarchy
- Drag and drop reordering
- Create/edit/delete modals
- Color and icon pickers
- Transaction count display

## Deliverables
- [ ] Category management page
- [ ] Tree view component
- [ ] CRUD modals
- [ ] Drag and drop
- [ ] Color/icon pickers

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.10)" \
  --label "task,phase-3,frontend"

gh issue create \
  --title "[Task 3.11] Date Range Picker" \
  --body "## Description
Create reusable date range picker component with presets.

## Requirements
- Preset options (This Month, Last Month, etc.)
- Custom range selection
- Calendar interface
- URL state persistence
- Keyboard navigation

## Deliverables
- [ ] DateRangePicker component
- [ ] Preset definitions
- [ ] Calendar integration
- [ ] URL parameter handling
- [ ] Used in dashboard and transactions

## Related
- Phase: 3 (Dashboard)
- Prompt: prompts/phase-3-dashboard.md (Task 3.11)" \
  --label "task,phase-3,frontend"

# =============================================================================
# PHASE 4 TASKS (NLQ & Export)
# =============================================================================

echo "Creating Phase 4 task issues..."

gh issue create \
  --title "[Task 4.1] NLQ Prompt Engineering" \
  --body "## Description
Design the prompt template for natural language to SQL translation.

## Requirements
- NlqPromptBuilder service
- System prompt with database schema
- Profile-specific context (categories, accounts)
- Example Q&A pairs
- Output JSON format definition
- Safety rules (SELECT only)

## Deliverables
- [ ] NlqPromptBuilder service
- [ ] Comprehensive system prompt
- [ ] 10+ example Q&A pairs
- [ ] Output format schema

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.1)" \
  --label "task,phase-4,backend,llm"

gh issue create \
  --title "[Task 4.2] SQL Validation" \
  --body "## Description
Create SQL validation service to ensure safe query execution.

## Requirements
- SqlValidator service
- Whitelist SELECT statements only
- Block dangerous operations
- Ensure profile_id filtering
- Add LIMIT if missing
- Parameter injection

## Deliverables
- [ ] SqlValidator service
- [ ] Tokenizer/parser
- [ ] Whitelist logic
- [ ] Query rewriting
- [ ] Unit tests

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.2)" \
  --label "task,phase-4,backend,security"

gh issue create \
  --title "[Task 4.3] NLQ Execution" \
  --body "## Description
Implement NLQ execution flow from question to result.

## Requirements
- ExecuteNlq command
- Full execution pipeline
- LLM integration
- SQL validation and execution
- Result formatting
- Error handling

## Deliverables
- [ ] ExecuteNlq command and handler
- [ ] NlqResult DTO
- [ ] Execution endpoint
- [ ] Comprehensive error handling

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.3)" \
  --label "task,phase-4,backend,llm"

gh issue create \
  --title "[Task 4.4] Results Rendering" \
  --body "## Description
Create flexible result rendering based on result type.

## Requirements
- Scalar result renderer
- Table result renderer
- Chart result renderer
- Auto-detect result type
- Column type detection and formatting

## Deliverables
- [ ] NlqResultRenderer component
- [ ] Scalar renderer
- [ ] Table renderer
- [ ] Chart renderer
- [ ] SQL display toggle

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.4)" \
  --label "task,phase-4,frontend"

gh issue create \
  --title "[Task 4.5] NLQ UI" \
  --body "## Description
Create the natural language query UI.

## Requirements
- NLQ page or sidebar panel
- NlqInput with examples
- NlqHistory component
- NlqSuggestions
- Loading and error states
- Keyboard shortcut (Cmd+K)

## Deliverables
- [ ] NLQ page (/ask)
- [ ] Input component
- [ ] History sidebar
- [ ] Suggestions component
- [ ] Keyboard navigation

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.5)" \
  --label "task,phase-4,frontend"

gh issue create \
  --title "[Task 4.6] Budget Suggestions" \
  --body "## Description
Implement LLM-powered budget suggestion feature.

## Requirements
- GetBudgetSuggestion query
- Analyze spending patterns (3-6 months)
- Calculate averages per category
- Identify trends
- LLM prompt for recommendations

## Deliverables
- [ ] GetBudgetSuggestion query
- [ ] Budget analysis prompt
- [ ] BudgetSuggestionDto
- [ ] Suggestion endpoint

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.6)" \
  --label "task,phase-4,backend,llm,enhancement"

gh issue create \
  --title "[Task 4.7] JSON Export" \
  --body "## Description
Implement full data export in JSON format for portability.

## Requirements
- ExportProfile query
- Export all profile data (accounts, categories, rules, transactions)
- Versioned format
- Date range filtering
- File download endpoint

## Deliverables
- [ ] ExportProfile query
- [ ] JSON export format v1.0
- [ ] Export endpoint with download
- [ ] Optional filters (date range, etc.)

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.7)" \
  --label "task,phase-4,backend,export"

gh issue create \
  --title "[Task 4.8] CSV Export" \
  --body "## Description
Implement transaction export to CSV for spreadsheet compatibility.

## Requirements
- ExportTransactionsCsv query
- Excel/Google Sheets compatibility
- UTF-8 with BOM
- Configurable columns
- Filtering support

## Deliverables
- [ ] ExportTransactionsCsv query
- [ ] CSV format with proper encoding
- [ ] Export endpoint
- [ ] Column selection
- [ ] Filter integration

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.8)" \
  --label "task,phase-4,backend,export"

gh issue create \
  --title "[Task 4.9] JSON Import" \
  --body "## Description
Implement data import from JSON export files.

## Requirements
- ImportProfile command
- Parse and validate JSON structure
- ID remapping (regenerate UUIDs)
- Preserve relationships
- Preview before import

## Deliverables
- [ ] ImportProfile command
- [ ] Validation logic
- [ ] ID remapping service
- [ ] Import endpoint with preview
- [ ] Confirmation flow

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.9)" \
  --label "task,phase-4,backend,import"

gh issue create \
  --title "[Task 4.10] Export/Import UI" \
  --body "## Description
Create export and import UI for data portability.

## Requirements
- Export settings page (/settings/export)
- Import page (/settings/import)
- ExportPanel with type selector
- ImportPanel with preview
- Progress indicators

## Deliverables
- [ ] Export page
- [ ] Import page
- [ ] File upload component
- [ ] ImportPreview component
- [ ] Progress indicators

## Related
- Phase: 4 (NLQ & Export)
- Prompt: prompts/phase-4-nlq-export.md (Task 4.10)" \
  --label "task,phase-4,frontend"

# =============================================================================
# PHASE 5 TASKS (Polish)
# =============================================================================

echo "Creating Phase 5 task issues..."

gh issue create \
  --title "[Task 5.1] Global Error Handling" \
  --body "## Description
Implement comprehensive error handling across backend and frontend.

## Requirements
- Custom exception types
- Global exception handler middleware
- Problem Details (RFC 7807) format
- Structured logging
- Frontend error boundary
- Toast notifications

## Deliverables
- [ ] Exception types
- [ ] Exception middleware
- [ ] Problem Details responses
- [ ] Error boundary component
- [ ] Toast integration

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.1)" \
  --label "task,phase-5,backend,frontend,quality"

gh issue create \
  --title "[Task 5.2] Loading States" \
  --body "## Description
Improve loading states throughout the application.

## Requirements
- Consistent skeleton loaders
- Optimistic updates
- Progress indicators
- React Suspense integration
- Minimum display time to avoid flicker

## Deliverables
- [ ] Skeleton components library
- [ ] Optimistic update hooks
- [ ] Progress components
- [ ] Suspense boundaries

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.2)" \
  --label "task,phase-5,frontend,quality"

gh issue create \
  --title "[Task 5.3] Database Performance" \
  --body "## Description
Optimize database queries and add performance improvements.

## Requirements
- Query optimization review
- Add missing indexes
- Implement caching (memory cache)
- Compiled queries for hot paths
- Connection pooling configuration
- Performance tests

## Deliverables
- [ ] Optimized queries
- [ ] New indexes migration
- [ ] Caching implementation
- [ ] Performance test suite
- [ ] Before/after metrics

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.3)" \
  --label "task,phase-5,backend,database,performance"

gh issue create \
  --title "[Task 5.4] Frontend Performance" \
  --body "## Description
Optimize frontend performance and bundle size.

## Requirements
- Code splitting (routes and components)
- Bundle optimization
- React optimization (memo, virtualization)
- Network optimization (prefetch, compression)
- Asset optimization (images, WebP)
- Lighthouse audit (target >90)

## Deliverables
- [ ] Lazy-loaded routes
- [ ] Virtualized lists
- [ ] Bundle size reduction
- [ ] Lighthouse score >90
- [ ] Performance report

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.4)" \
  --label "task,phase-5,frontend,performance"

gh issue create \
  --title "[Task 5.5] Logging and Monitoring" \
  --body "## Description
Implement comprehensive structured logging.

## Requirements
- Serilog integration
- Structured logging (JSON in production)
- Correlation IDs
- Log key business events
- Performance logging
- Health endpoints

## Deliverables
- [ ] Serilog configuration
- [ ] Structured log events
- [ ] Health check endpoints
- [ ] Logging documentation

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.5)" \
  --label "task,phase-5,backend,monitoring"

gh issue create \
  --title "[Task 5.6] Documentation" \
  --body "## Description
Create comprehensive project documentation.

## Requirements
- README.md with quick start
- ARCHITECTURE.md
- API documentation (OpenAPI/Swagger)
- User guides (import, rules, NLQ)
- Rules syntax reference
- Deployment guide

## Deliverables
- [ ] Updated README.md
- [ ] ARCHITECTURE.md
- [ ] OpenAPI spec
- [ ] User guides
- [ ] Deployment guide

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.6)" \
  --label "task,phase-5,documentation"

gh issue create \
  --title "[Task 5.7] UI Polish" \
  --body "## Description
Polish the user interface for consistency and accessibility.

## Requirements
- Consistent styling review
- Responsive design on mobile
- Accessibility (WCAG AA)
- Micro-interactions
- Empty states for all views
- Dark mode support

## Deliverables
- [ ] Style consistency pass
- [ ] Mobile responsive layouts
- [ ] Accessibility improvements
- [ ] Dark mode toggle
- [ ] Empty state components

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.7)" \
  --label "task,phase-5,frontend,quality,a11y"

gh issue create \
  --title "[Task 5.8] Testing Coverage" \
  --body "## Description
Increase test coverage across backend and frontend.

## Requirements
- Unit tests for all services
- Integration tests for all endpoints
- Frontend component tests
- E2E tests for critical flows (optional)
- Coverage targets: Backend >80%, Frontend >70%

## Deliverables
- [ ] Additional unit tests
- [ ] Integration test suite
- [ ] Frontend component tests
- [ ] Coverage reports
- [ ] Test documentation

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.8)" \
  --label "task,phase-5,testing,quality"

gh issue create \
  --title "[Task 5.9] Security Review" \
  --body "## Description
Conduct comprehensive security review and fixes.

## Requirements
- Authentication/Authorization review
- Input validation audit
- SQL injection prevention check
- XSS prevention check
- File upload security
- Dependency audit (dotnet/npm)

## Deliverables
- [ ] Security checklist
- [ ] Fixed vulnerabilities
- [ ] Security documentation
- [ ] Audit reports

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.9)" \
  --label "task,phase-5,security,quality"

gh issue create \
  --title "[Task 5.10] Final Testing and Bug Fixes" \
  --body "## Description
Final testing phase and bug fixing before release.

## Requirements
- Complete user journey testing
- Edge case testing
- Browser compatibility testing
- Mobile testing
- Performance testing
- Pre-release checklist

## Deliverables
- [ ] Bug fix list
- [ ] Test reports
- [ ] Release notes
- [ ] Pre-release validation
- [ ] Deployment checklist

## Related
- Phase: 5 (Polish)
- Prompt: prompts/phase-5-polish.md (Task 5.10)" \
  --label "task,phase-5,testing,quality"

echo ""
echo "========================================="
echo "All issues created successfully!"
echo "========================================="
echo ""
echo "Summary:"
echo "  - 8 Import module user stories"
echo "  - 9 Rules Engine module user stories"
echo "  - 10 Transactions module user stories"
echo "  - 8 Dashboard module user stories"
echo "  - 7 NLQ module user stories"
echo "  - 11 Phase 2 tasks"
echo "  - 11 Phase 3 tasks"
echo "  - 10 Phase 4 tasks"
echo "  - 10 Phase 5 tasks"
echo ""
echo "Total: 84 GitHub issues created!"
echo ""
