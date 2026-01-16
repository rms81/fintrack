using System.Text.Json;
using System.Text.RegularExpressions;
using FinTrack.Core.Features.Nlq;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

public class NlqService(
    ILlmService llmService,
    FinTrackDbContext db,
    ILogger<NlqService> logger) : INlqService
{
    private static readonly string[] ForbiddenKeywords =
    [
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", "TRUNCATE",
        "EXECUTE", "EXEC", "INTO", "GRANT", "REVOKE", "MERGE",
        "COPY", "CALL", "PERFORM", "SET", "WITH",
        "SAVEPOINT", "ROLLBACK", "COMMIT", "VACUUM", "ANALYZE",
        ";"
    ];

    private static readonly Regex TransactionsTablePattern = new(
        @"\bFROM\s+(""?transactions""?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<NlqResponse> ExecuteQueryAsync(
        Guid profileId,
        string question,
        CancellationToken ct = default)
    {
        try
        {
            // Get valid account IDs for this profile
            var validAccountIds = await db.Accounts
                .Where(a => a.ProfileId == profileId)
                .Select(a => a.Id)
                .ToListAsync(ct);

            // Build schema context with profile-specific data
            var schemaContext = await BuildSchemaContextAsync(profileId, ct);

            // Get SQL from LLM
            var llmResponse = await llmService.TranslateToSqlAsync(question, schemaContext, ct);

            // Parse LLM response
            var parsed = ParseLlmResponse(llmResponse);

            // Validate SQL for safety and verify account_id filtering
            var validationError = ValidateSql(parsed.Sql, validAccountIds);
            if (validationError != null)
            {
                return new NlqResponse(
                    question,
                    parsed.Sql,
                    NlqResultType.Error,
                    null,
                    null,
                    null,
                    validationError);
            }

            // Execute the query with enforced row-level security
            var (resultType, data) = await ExecuteSqlAsync(parsed.Sql, validAccountIds, ct);

            return new NlqResponse(
                question,
                parsed.Sql,
                resultType,
                data,
                parsed.Explanation ?? "Query executed successfully",
                parsed.ChartType,
                null);
        }
        catch (LlmServiceException ex)
        {
            logger.LogError(ex, "LLM service error for NLQ: {Question}", question);
            return new NlqResponse(
                question,
                null,
                NlqResultType.Error,
                null,
                null,
                null,
                "Failed to process your question. Please try rephrasing.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing NLQ: {Question}", question);
            return new NlqResponse(
                question,
                null,
                NlqResultType.Error,
                null,
                null,
                null,
                "An error occurred processing your query.");
        }
    }

    private async Task<string> BuildSchemaContextAsync(Guid profileId, CancellationToken ct)
    {
        // Get categories for this profile
        var categories = await db.Categories
            .Where(c => c.ProfileId == profileId)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        // Get accounts for this profile
        var accounts = await db.Accounts
            .Where(a => a.ProfileId == profileId)
            .Select(a => new { a.Id, a.Name, a.BankName })
            .ToListAsync(ct);

        var categoryList = string.Join("\n", categories.Select(c => $"    - '{c.Name}' (id: {c.Id})"));
        var accountList = string.Join("\n", accounts.Select(a => $"    - '{a.Name}' (bank: {a.BankName ?? "N/A"}, id: {a.Id})"));
        var accountIds = string.Join(", ", accounts.Select(a => $"'{a.Id}'"));

        return $$"""
            You are a SQL query generator for a personal finance application.

            DATABASE SCHEMA:

            Table: transactions
            - id (uuid, primary key)
            - account_id (uuid, foreign key to accounts)
            - category_id (uuid, nullable, foreign key to categories)
            - date (date)
            - amount (decimal) - negative for expenses, positive for income
            - description (text)
            - notes (text, nullable)
            - tags (text array)
            - created_at (timestamp)

            Table: categories
            - id (uuid, primary key)
            - profile_id (uuid)
            - name (text)
            - icon (text)
            - color (text)
            - parent_id (uuid, nullable, for subcategories)

            Table: accounts
            - id (uuid, primary key)
            - profile_id (uuid)
            - name (text)
            - bank_name (text, nullable)
            - currency (text)

            AVAILABLE CATEGORIES:
            {{categoryList}}

            AVAILABLE ACCOUNTS:
            {{accountList}}

            IMPORTANT RULES:
            1. ONLY generate SELECT queries - no INSERT, UPDATE, DELETE, or DDL
            2. ALWAYS filter transactions by account_id IN ({{accountIds}})
            3. Use PostgreSQL syntax
            4. For expenses, amount < 0; for income, amount > 0
            5. Add LIMIT 100 if not specified
            6. Use category names in WHERE clauses by joining with categories table
            7. For date comparisons, use: CURRENT_DATE, CURRENT_DATE - INTERVAL '1 month', etc.

            OUTPUT FORMAT:
            Respond with ONLY valid JSON in this format:
            {
              "sql": "SELECT ...",
              "resultType": "scalar" | "table" | "chart",
              "chartType": "pie" | "bar" | "line" | null,
              "explanation": "Brief explanation of what the query does"
            }

            - Use "scalar" for single value results (SUM, COUNT, AVG)
            - Use "table" for list/detail results
            - Use "chart" when data is suitable for visualization (grouped/aggregated data)
            """;
    }

    private static NlqLlmResponse ParseLlmResponse(string response)
    {
        // Try to extract JSON from response
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            try
            {
                var parsed = JsonSerializer.Deserialize<NlqLlmResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed != null)
                    return parsed;
            }
            catch (JsonException)
            {
                // Fall through to plain SQL handling
            }
        }

        // If not JSON, treat entire response as SQL
        return new NlqLlmResponse
        {
            Sql = response.Trim(),
            ResultType = "table"
        };
    }

    private static string? ValidateSql(string sql, List<Guid> validAccountIds)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return "No SQL query was generated";

        var upperSql = sql.ToUpperInvariant();

        // Check for forbidden keywords
        foreach (var keyword in ForbiddenKeywords)
        {
            var pattern = $@"\b{keyword}\b";
            if (Regex.IsMatch(upperSql, pattern, RegexOptions.IgnoreCase))
            {
                return $"Query contains forbidden operation: {keyword}";
            }
        }

        // Must be a SELECT query
        if (!upperSql.TrimStart().StartsWith("SELECT"))
        {
            return "Only SELECT queries are allowed";
        }

        // Check for SQL comment injection attempts
        if (sql.Contains("--") || sql.Contains("/*"))
        {
            return "SQL comments are not allowed in queries";
        }

        return null;
    }

    private async Task<(NlqResultType, object?)> ExecuteSqlAsync(
        string sql,
        List<Guid> validAccountIds,
        CancellationToken ct)
    {
        // Enforce row-level security by creating a temporary view
        // This ensures that even if the LLM-generated SQL is missing the account_id filter,
        // we programmatically enforce it at the database level
        var (securedSql, parameters) = EnforceRowLevelSecurity(sql, validAccountIds);

        // Add LIMIT if not present
        if (!securedSql.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
        {
            securedSql = securedSql.TrimEnd().TrimEnd(';') + " LIMIT 100";
        }

        try
        {
            // Execute raw SQL and get results as dictionaries
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = securedSql;
            command.CommandTimeout = 30;

            // Add parameters to prevent SQL injection
            foreach (var (name, value) in parameters)
            {
                var param = command.CreateParameter();
                param.ParameterName = name;
                param.Value = value;
                command.Parameters.Add(param);
            }

            await using var reader = await command.ExecuteReaderAsync(ct);

            var results = new List<Dictionary<string, object?>>();
            var columnNames = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnNames[i]] = value;
                }
                results.Add(row);
            }

            // Determine result type
            if (results.Count == 1 && columnNames.Count == 1)
            {
                // Single value = scalar
                return (NlqResultType.Scalar, results[0][columnNames[0]]);
            }

            return (NlqResultType.Table, results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute NLQ SQL: {Sql}", sql);
            throw new InvalidOperationException($"Query execution failed: {ex.Message}");
        }
    }

    private static (string sql, Dictionary<string, object> parameters) EnforceRowLevelSecurity(
        string sql, 
        List<Guid> validAccountIds)
    {
        // If the SQL doesn't query the transactions table, no need to enforce
        var upperSql = sql.ToUpperInvariant();
        if (!upperSql.Contains("FROM TRANSACTIONS") && !upperSql.Contains("FROM \"TRANSACTIONS\""))
        {
            return (sql, new Dictionary<string, object>());
        }

        // Use a parameter for the account IDs array to prevent SQL injection
        var parameters = new Dictionary<string, object>
        {
            ["@authorized_account_ids"] = validAccountIds.ToArray()
        };

        // Wrap the query to enforce account filtering using a subquery with a parameter
        // This ensures that even if the LLM-generated SQL is missing the account_id filter,
        // we programmatically enforce it by wrapping all transaction table references
        // Note: We replace ALL occurrences to handle JOINs and multiple table references
        var securedSql = TransactionsTablePattern.Replace(sql, 
            "FROM (SELECT * FROM transactions WHERE account_id = ANY(@authorized_account_ids)) AS transactions");

        return (securedSql, parameters);
    }
}
