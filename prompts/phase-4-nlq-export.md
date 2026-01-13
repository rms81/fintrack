# Phase 4: Natural Language Queries & Export

## Duration: Week 7-8

## Overview
Implement natural language querying with SQL generation, and data export/import functionality for portability.

## Goals
1. Build NLQ interface with LLM
2. Implement SQL generation and validation
3. Create results rendering
4. Build JSON export for full backup
5. Build CSV export for spreadsheets
6. Implement JSON import

---

## Task 4.1: NLQ Prompt Engineering

### Prompt
```
Design the prompt template for natural language to SQL translation:

1. Create NlqPromptBuilder service:
   - Build system prompt with database schema
   - Include profile context (categories, accounts)
   - Provide examples of questions and SQL
   - Define output JSON format

2. System prompt should include:
   - Table definitions (transactions, categories, accounts)
   - Column types and constraints
   - Profile-specific data (category names, account names)
   - Safety rules (SELECT only, no mutations)

3. Output format:
   {
     "sql": "SELECT ...",
     "parameters": { "param1": value },
     "resultType": "scalar" | "table" | "chart" | "error",
     "chartType": "pie" | "bar" | "line" | null,
     "explanation": "Human readable explanation"
   }

4. Example Q&A pairs:
   - "How much did I spend on food last month?"
   - "What are my top 5 expenses?"
   - "Show me all Netflix transactions"
   - "Compare December vs November spending"
   - "What's my average monthly grocery spend?"

Test with various phrasings and edge cases.
```

### Expected Output
- NlqPromptBuilder service
- Comprehensive system prompt
- Example pairs

---

## Task 4.2: SQL Validation

### Prompt
```
Create SQL validation service to ensure safe queries:

1. SqlValidator service:
   - Parse SQL using a simple tokenizer
   - Whitelist only SELECT statements
   - Block: INSERT, UPDATE, DELETE, DROP, ALTER, CREATE, TRUNCATE
   - Block: INTO, EXECUTE, EXEC
   - Ensure profile_id filter is present
   - Limit result count (add LIMIT if missing)

2. Parameter injection:
   - Replace @ProfileId with actual profile ID
   - Validate other parameters against allowed types

3. Query rewriting:
   - Add LIMIT 1000 if not present
   - Wrap in subquery for safety if needed

4. Error messages:
   - Clear explanation of why query was rejected
   - Suggest alternatives
```

### Expected Output
- SqlValidator service
- Whitelist logic
- Parameter handling

---

## Task 4.3: NLQ Execution

### Prompt
```
Implement NLQ execution flow:

1. Create ExecuteNlq command:
   Input: { profileId, question, conversationId? }
   
   Flow:
   a. Build prompt with schema context
   b. Call OpenRouter LLM
   c. Parse response JSON
   d. Validate generated SQL
   e. Execute query via EF Core raw SQL
   f. Format results based on resultType
   g. Return NlqResult

2. NlqResult:
   - Question (original)
   - GeneratedSql (for transparency)
   - ResultType (scalar, table, chart, error)
   - Data (the actual result)
   - Explanation (from LLM)
   - ChartConfig (if chart type)
   - ErrorMessage (if error)

3. Endpoint:
   POST /api/profiles/{profileId}/nlq/query

4. Handle errors gracefully:
   - LLM timeout
   - Invalid SQL generated
   - Query execution error
   - Empty results
```

### Expected Output
- ExecuteNlq command and handler
- Result formatting
- Error handling

---

## Task 4.4: Results Rendering

### Prompt
```
Create flexible results rendering based on result type:

1. Scalar results:
   - Single value display
   - Format as currency if amount
   - Show with comparison if temporal

2. Table results:
   - DataTable component
   - Auto-detect column types
   - Format dates and currency
   - Sortable columns

3. Chart results:
   - Detect chart type from LLM suggestion
   - Render appropriate Recharts component
   - Auto-configure based on data shape

4. Create NlqResultRenderer component:
   - Props: { result: NlqResult }
   - Render appropriate visualization
   - Show "Show SQL" collapsible
   - Copy results to clipboard
```

### Expected Output
- Result type detection
- Scalar, table, chart renderers
- Unified renderer component

---

## Task 4.5: NLQ UI

### Prompt
```
Create the Natural Language Query UI:

Page: /ask or sidebar panel

Components:
1. NlqInput
   - Large text input
   - Submit button
   - Voice input (optional)
   - Example questions dropdown

2. NlqHistory
   - List of past questions
   - Click to re-run
   - Clear history

3. NlqResult
   - Show question
   - Show result (using NlqResultRenderer)
   - Show generated SQL (collapsible)
   - Feedback buttons (helpful/not helpful)

4. NlqSuggestions
   - Show suggested follow-up questions
   - Based on current result

Features:
- Loading state with typing animation
- Error state with retry
- Keyboard shortcut to focus (Cmd+K)
- Conversation context (follow-up questions)
```

### Expected Output
- NLQ page/panel
- Input and history
- Result display
- Suggestions

---

## Task 4.6: Budget Suggestions

### Prompt
```
Implement budget suggestion feature:

1. Create GetBudgetSuggestion query:
   - Analyze spending patterns over 3-6 months
   - Calculate average per category
   - Identify trends (increasing/decreasing)
   - Suggest realistic budget

2. Prompt engineering:
   - Provide historical spending data
   - Ask for budget recommendations
   - Consider income if available
   - Be conservative (add buffer)

3. Response:
   {
     "categories": [
       {
         "category": "Food",
         "averageMonthly": 450,
         "suggestedBudget": 500,
         "trend": "stable",
         "notes": "Consider meal planning to reduce restaurant spending"
       }
     ],
     "totalSuggestedBudget": 2500,
     "insights": ["Your subscriptions have increased 15% in 3 months"]
   }

4. Endpoint:
   POST /api/profiles/{profileId}/nlq/budget-suggestion
```

### Expected Output
- Budget analysis query
- LLM prompt for suggestions
- Response structure

---

## Task 4.7: JSON Export

### Prompt
```
Implement full data export in JSON format:

1. Create ExportProfile query:
   - Export all profile data
   - Portable format for import to another instance

2. Export structure:
   {
     "version": "1.0",
     "exportedAt": "2024-01-15T10:30:00Z",
     "profile": {
       "name": "Personal",
       "type": "Personal"
     },
     "accounts": [...],
     "categories": [...],
     "rules": "# TOML content...",
     "importFormats": [...],
     "transactions": [...]
   }

3. Endpoint:
   GET /api/profiles/{profileId}/export/json
   - Returns JSON file download
   - Filename: fintrack-{profileName}-{date}.json
   - Content-Disposition header for download

4. Options:
   - dateRange (optional, for partial export)
   - includeRules (default true)
   - includeFormats (default true)
```

### Expected Output
- Export query
- JSON structure
- File download endpoint

---

## Task 4.8: CSV Export

### Prompt
```
Implement transaction export to CSV:

1. Create ExportTransactionsCsv query:
   - Export transactions as CSV
   - Compatible with Excel/Google Sheets

2. CSV columns:
   - Date, Description, Amount, Category, Subcategory, Tags, Account, Notes

3. Endpoint:
   GET /api/profiles/{profileId}/export/csv
   - Query params for filtering (same as transaction list)
   - Returns CSV file download
   - UTF-8 with BOM for Excel compatibility

4. Options:
   - columns (select which columns)
   - dateRange
   - accountIds
   - categoryIds
```

### Expected Output
- CSV export query
- Excel-compatible format
- Filtering options

---

## Task 4.9: JSON Import

### Prompt
```
Implement data import from JSON export:

1. Create ImportProfile command:
   - Accept JSON export file
   - Parse and validate structure
   - Handle ID conflicts (regenerate all IDs)
   - Preserve relationships

2. Import flow:
   a. Upload JSON file
   b. Parse and validate
   c. Show preview (counts of each entity type)
   d. User confirms
   e. Create profile and all related data
   f. Return import result

3. Endpoint:
   POST /api/import/json
   - Multipart form with file
   - Returns preview first, then confirm

4. ID remapping:
   - Generate new UUIDv7 for all entities
   - Maintain internal references
   - Profile gets new ID
   - User ID is current authenticated user
```

### Expected Output
- Import command
- ID remapping logic
- Preview and confirm flow

---

## Task 4.10: Export/Import UI

### Prompt
```
Create Export/Import UI:

Pages:
- /settings/export
- /settings/import

Components:
1. ExportPanel
   - Export type selector (JSON/CSV)
   - Date range picker (for CSV)
   - Column selector (for CSV)
   - Download button
   - Progress indicator

2. ImportPanel
   - File upload zone
   - File type detection
   - Preview of what will be imported
   - Conflict warnings
   - Confirm button

3. ImportPreview
   - Show counts: X accounts, Y categories, Z transactions
   - Show any warnings
   - Option to skip certain data

Features:
- Progress for large exports
- Download ready notification
- Import history
```

### Expected Output
- Export settings page
- Import wizard
- Preview components

---

## Completion Criteria

Phase 4 is complete when:
- [ ] NLQ prompt generates valid SQL
- [ ] SQL validation blocks dangerous queries
- [ ] Results render correctly (scalar, table, chart)
- [ ] NLQ UI allows asking questions
- [ ] Follow-up questions work
- [ ] Budget suggestions generate
- [ ] JSON export downloads complete data
- [ ] CSV export works with filters
- [ ] JSON import creates new profile
- [ ] ID conflicts are handled
- [ ] All tests pass

## Next Phase
Proceed to Phase 5: Polish
