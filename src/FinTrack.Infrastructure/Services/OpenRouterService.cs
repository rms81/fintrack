using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinTrack.Core.Domain.ValueObjects;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinTrack.Infrastructure.Services;

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

    public async Task<string> TranslateToSqlAsync(
        string naturalLanguageQuery,
        string schemaContext,
        CancellationToken ct = default)
    {
        var prompt = BuildNlqPrompt(naturalLanguageQuery, schemaContext);
        return await SendCompletionAsync(prompt, ct);
    }

    private async Task<string> SendCompletionAsync(string prompt, CancellationToken ct)
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

        try
        {
            var response = await httpClient.PostAsJsonAsync("chat/completions", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("OpenRouter API error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new LlmRateLimitException("LLM service rate limited, try again later");
                }

                throw new LlmServiceException($"LLM API error: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(ct);
            if (result?.Choices is null || result.Choices.Length == 0)
            {
                throw new LlmServiceException("Empty response from LLM");
            }

            return result.Choices[0].Message.Content;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to call OpenRouter API");
            throw new LlmServiceException("Failed to connect to LLM service", ex);
        }
    }

    private static string GetSystemPrompt() => """
        You are a data analysis assistant specializing in financial data.
        You respond ONLY with valid JSON when asked to analyze data.
        You never include explanations outside of the JSON structure.
        """;

    private static string BuildCsvDetectionPrompt(string csvSample) => $$"""
        Analyze this CSV bank statement sample and identify the column mappings.

        CSV Sample (first rows):
        {{csvSample}}

        Identify:
        1. Date column index (0-based) and format (e.g., "dd/MM/yyyy")
        2. Description column index
        3. Amount column(s) - could be single signed amount or separate debit/credit
        4. Balance column index (if present)
        5. Delimiter used
        6. Whether first row is header

        Respond ONLY with this JSON format (no other text):
        {
          "delimiter": ",",
          "hasHeader": true,
          "dateColumn": 0,
          "dateFormat": "dd/MM/yyyy",
          "descriptionColumn": 1,
          "amountType": "signed",
          "amountColumn": 2,
          "debitColumn": null,
          "creditColumn": null,
          "balanceColumn": null
        }

        amountType must be either "signed" (single column with positive/negative values) or "split" (separate debit and credit columns).
        """;

    private static string BuildNlqPrompt(string query, string schema) => $"""
        Convert this natural language query to SQL:

        User query: "{query}"

        {schema}

        Respond with only the SQL query, no explanation.
        """;

    private static CsvFormatConfig ParseCsvFormatResponse(string response)
    {
        // Extract JSON from response (in case there's extra text)
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd < 0 || jsonEnd <= jsonStart)
        {
            throw new LlmParseException("Could not find JSON in LLM response");
        }

        var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);

        try
        {
            var parsed = JsonSerializer.Deserialize<CsvFormatResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed is null)
            {
                throw new LlmParseException("Failed to parse CSV format from response");
            }

            return new CsvFormatConfig
            {
                Delimiter = parsed.Delimiter ?? ",",
                HasHeader = parsed.HasHeader,
                DateColumn = parsed.DateColumn,
                DateFormat = parsed.DateFormat ?? "yyyy-MM-dd",
                DescriptionColumn = parsed.DescriptionColumn,
                AmountType = parsed.AmountType ?? "signed",
                AmountColumn = parsed.AmountColumn,
                DebitColumn = parsed.DebitColumn,
                CreditColumn = parsed.CreditColumn,
                BalanceColumn = parsed.BalanceColumn
            };
        }
        catch (JsonException ex)
        {
            throw new LlmParseException("Invalid JSON in LLM response", ex);
        }
    }

    private record CsvFormatResponse
    {
        public string? Delimiter { get; init; }
        public bool HasHeader { get; init; }
        public int DateColumn { get; init; }
        public string? DateFormat { get; init; }
        public int DescriptionColumn { get; init; }
        public string? AmountType { get; init; }
        public int? AmountColumn { get; init; }
        public int? DebitColumn { get; init; }
        public int? CreditColumn { get; init; }
        public int? BalanceColumn { get; init; }
    }

    private record OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public Choice[] Choices { get; init; } = [];
    }

    private record Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; init; } = null!;
    }

    private record Message
    {
        [JsonPropertyName("content")]
        public string Content { get; init; } = "";
    }
}

public class LlmServiceException(string message, Exception? inner = null)
    : Exception(message, inner);

public class LlmRateLimitException(string message)
    : LlmServiceException(message);

public class LlmParseException(string message, Exception? inner = null)
    : LlmServiceException(message, inner);
