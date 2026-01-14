# Import Module

> **Phase:** 2 (Import & Rules)
> **Status:** Planned

## Overview
CSV import with LLM-assisted format detection for bank statements.

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
