# OpenRouter Integration

## Overview
FinTrack uses OpenRouter as an LLM gateway for:
1. **CSV Format Detection** - Automatically detect column mappings in bank statements
2. **Natural Language Queries** - Translate user questions to SQL

## Configuration

### Environment Variables
```bash
Llm__ApiKey=sk-or-v1-xxx
Llm__Model=openai/gpt-4-turbo
Llm__BaseUrl=https://openrouter.ai/api/v1
```

### appsettings.json
```json
{
  "Llm": {
    "ApiKey": "",
    "Model": "openai/gpt-4-turbo",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "MaxTokens": 1000,
    "Temperature": 0.1
  }
}
```

### Options Class
```csharp
public class LlmOptions
{
    public const string Section = "Llm";
    
    public required string ApiKey { get; init; }
    public string Model { get; init; } = "openai/gpt-4-turbo";
    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1";
    public int MaxTokens { get; init; } = 1000;
    public float Temperature { get; init; } = 0.1f;
}
```

## Service Implementation

### ILlmService Interface
```csharp
public interface ILlmService
{
    Task<CsvFormatConfig> DetectCsvFormatAsync(
        string csvSample, 
        CancellationToken ct = default);
        
    Task<string> TranslateToSqlAsync(
        string naturalLanguageQuery,
        string schemaContext,
        CancellationToken ct = default);
}
```

### OpenRouterService
```csharp
public class OpenRouterService(
    HttpClient httpClient,
    IOptions<LlmOptions> options,
    ILogger<OpenRouterService> logger) : ILlmService
{
    private readonly LlmOptions _options = options.Value;

    public async Task<CsvFormatConfig> DetectCsvFormatAsync(
        string csvSample, 
        CancellationToken ct = default)
    {
        var prompt = BuildCsvDetectionPrompt(csvSample);
        var response = await SendCompletionAsync(prompt, ct);
        return ParseCsvFormatResponse(response);
    }

    private async Task<string> SendCompletionAsync(
        string prompt, 
        CancellationToken ct)
    {
        var request = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = prompt }
            },
            max_tokens = _options.MaxTokens,
            temperature = _options.Temperature
        };

        var response = await httpClient.PostAsJsonAsync(
            "chat/completions", 
            request, 
            ct);

        response.EnsureSuccessStatusCode();
        
        var result = await response.Content
            .ReadFromJsonAsync<OpenRouterResponse>(ct);
            
        return result!.Choices[0].Message.Content;
    }
}
```

## CSV Format Detection

### Prompt Template
```csharp
private static string BuildCsvDetectionPrompt(string csvSample) => $"""
    Analyze this CSV bank statement sample and identify the column mappings.
    
    CSV Sample (first 5 rows):
    {csvSample}
    
    Identify:
    1. Date column (and format, e.g., "dd/MM/yyyy")
    2. Description column
    3. Amount column(s) - could be single signed amount or separate debit/credit
    4. Balance column (if present)
    5. Delimiter used
    6. Whether first row is header
    
    Respond in JSON format:
    {{
      "delimiter": ",",
      "hasHeader": true,
      "dateColumn": 0,
      "dateFormat": "dd/MM/yyyy",
      "descriptionColumn": 1,
      "amountType": "signed" | "split",
      "amountColumn": 2,
      "debitColumn": null,
      "creditColumn": null,
      "balanceColumn": 3
    }}
    """;
```

### Response Model
```csharp
public record CsvFormatConfig
{
    public string Delimiter { get; init; } = ",";
    public bool HasHeader { get; init; } = true;
    public int DateColumn { get; init; }
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public int DescriptionColumn { get; init; }
    public string AmountType { get; init; } = "signed";
    public int? AmountColumn { get; init; }
    public int? DebitColumn { get; init; }
    public int? CreditColumn { get; init; }
    public int? BalanceColumn { get; init; }
}
```

## Natural Language Queries

### Schema Context
```csharp
private static string GetSchemaContext(Guid profileId) => $"""
    Available tables for profile {profileId}:
    
    transactions (
      id uuid,
      account_id uuid,
      category_id uuid,
      date date,
      amount decimal,
      description text,
      tags text[]
    )
    
    categories (
      id uuid,
      name text,
      parent_id uuid
    )
    
    accounts (
      id uuid,
      name text,
      institution text
    )
    
    Rules:
    - Always filter by profile's accounts
    - Use PostgreSQL syntax
    - Return only SELECT queries
    - Limit results to 100 rows
    """;
```

### NLQ Prompt
```csharp
private static string BuildNlqPrompt(string query, string schema) => $"""
    Convert this natural language query to SQL:
    
    User query: "{query}"
    
    {schema}
    
    Respond with only the SQL query, no explanation.
    """;
```

## DI Registration

```csharp
// Program.cs
builder.Services.Configure<LlmOptions>(
    builder.Configuration.GetSection(LlmOptions.Section));

builder.Services.AddHttpClient<ILlmService, OpenRouterService>(client =>
{
    var options = builder.Configuration
        .GetSection(LlmOptions.Section)
        .Get<LlmOptions>()!;
        
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
    client.DefaultRequestHeaders.Add("HTTP-Referer", "https://fintrack.local");
});
```

## Error Handling

```csharp
public async Task<CsvFormatConfig> DetectCsvFormatAsync(
    string csvSample, 
    CancellationToken ct = default)
{
    try
    {
        var prompt = BuildCsvDetectionPrompt(csvSample);
        var response = await SendCompletionAsync(prompt, ct);
        return ParseCsvFormatResponse(response);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        logger.LogWarning("OpenRouter rate limit hit");
        throw new ServiceUnavailableException("LLM service rate limited, try again later");
    }
    catch (JsonException ex)
    {
        logger.LogError(ex, "Failed to parse LLM response");
        throw new LlmParseException("Could not parse CSV format from LLM response");
    }
}
```

## Testing

### Mock Service
```csharp
public class MockLlmService : ILlmService
{
    public Task<CsvFormatConfig> DetectCsvFormatAsync(
        string csvSample, 
        CancellationToken ct = default)
    {
        // Return sensible defaults for testing
        return Task.FromResult(new CsvFormatConfig
        {
            Delimiter = ",",
            HasHeader = true,
            DateColumn = 0,
            DateFormat = "dd/MM/yyyy",
            DescriptionColumn = 1,
            AmountType = "signed",
            AmountColumn = 2
        });
    }
    
    public Task<string> TranslateToSqlAsync(
        string query, 
        string schema, 
        CancellationToken ct = default)
    {
        return Task.FromResult("SELECT * FROM transactions LIMIT 10");
    }
}
```

### Integration Test Setup
```csharp
// Use mock in tests
services.AddSingleton<ILlmService, MockLlmService>();
```

## Fallback Behavior

If LLM detection fails, the system falls back to manual column mapping UI.

```csharp
public async Task<CsvFormatConfig?> TryDetectFormatAsync(string csvSample)
{
    try
    {
        return await _llmService.DetectCsvFormatAsync(csvSample);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "LLM format detection failed, falling back to manual");
        return null; // Trigger manual mapping UI
    }
}
```

## Cost Considerations

- OpenRouter charges per token
- CSV detection: ~500 input + ~200 output tokens per request
- NLQ: ~300 input + ~100 output tokens per query
- Consider caching format configs per institution
- Rate limit: 60 requests/minute on most models
