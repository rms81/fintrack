# Import Module

> **Phase:** 2 (Import & Rules)
> **Status:** Planned

## Overview
CSV import with LLM-assisted format detection for bank statements.

## User Stories

| ID | As a... | I want to... | So that... |
|----|---------|--------------|------------|
| US-I1 | User | upload a CSV file from my bank | I can import my transactions |
| US-I2 | User | have the format auto-detected | I don't need to manually configure column mappings |
| US-I3 | User | preview transactions before importing | I can verify the data looks correct |
| US-I4 | User | adjust format settings if auto-detection is wrong | I can handle unusual CSV formats |
| US-I5 | User | be warned about duplicate transactions | I don't accidentally import the same data twice |
| US-I6 | User | see import progress and status | I know when the import is complete |
| US-I7 | User | have rules automatically applied during import | new transactions are categorized immediately |
| US-I8 | User | view my import history | I can track which files I've already imported |

## Flow

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────────┐
│  Upload CSV │ ──▶ │ Detect Format│ ──▶ │ Preview Data│ ──▶ │ Import Trans │
└─────────────┘     └──────────────┘     └─────────────┘     └──────────────┘
                          │
                          ▼
                    ┌──────────────┐
                    │ LLM Analysis │
                    └──────────────┘
```

## Domain Model

```csharp
public class ImportSession : BaseEntity
{
    public Guid AccountId { get; init; }
    public required string Filename { get; init; }
    public int RowCount { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public string? ErrorMessage { get; set; }
    public CsvFormatConfig? FormatConfig { get; set; }
    
    // Navigation
    public Account Account { get; init; } = null!;
}

public enum ImportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

## LLM Format Detection

<!-- TODO: Implement in Phase 2 -->

Uses OpenRouter to analyze CSV sample and detect:
- Delimiter (comma, semicolon, tab)
- Header row presence
- Date column and format
- Description column
- Amount column(s) - signed or split debit/credit
- Balance column

See: `integrations/openrouter.md`

## Endpoints

<!-- TODO: Phase 2 -->

### POST /api/accounts/{accountId}/import/upload
Upload CSV file, returns detected format.

### POST /api/accounts/{accountId}/import/preview
Preview parsed transactions before import.

### POST /api/accounts/{accountId}/import/confirm
Confirm import and create transactions.

## Duplicate Detection

<!-- TODO: Phase 2 -->

Strategy: Hash of (date + amount + description) to detect duplicates.

## React Components

<!-- TODO: Phase 2 -->

- FileUpload component
- Format configuration form
- Transaction preview table
- Import progress indicator
