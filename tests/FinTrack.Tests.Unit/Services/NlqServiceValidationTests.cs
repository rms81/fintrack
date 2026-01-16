using System.Reflection;
using FinTrack.Infrastructure.Services;
using Xunit;

namespace FinTrack.Tests.Unit.Services;

public class NlqServiceValidationTests
{
    private readonly MethodInfo _validateSqlMethod;

    public NlqServiceValidationTests()
    {
        var nlqServiceType = typeof(NlqService);
        _validateSqlMethod = nlqServiceType.GetMethod("ValidateSql",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not find ValidateSql method");
    }

    private string? ValidateSql(string sql)
    {
        return _validateSqlMethod.Invoke(null, new object[] { sql }) as string;
    }

    [Theory]
    [InlineData("SELECT * FROM transactions; -- DROP TABLE accounts;")]
    [InlineData("SELECT * FROM transactions WHERE id = '1' -- AND 1=1")]
    [InlineData("SELECT /* malicious comment */ * FROM transactions")]
    [InlineData("SELECT * FROM transactions /* inline */ WHERE 1=1")]
    public void ValidateSql_RejectsSqlComments(string sql)
    {
        var result = ValidateSql(sql);
        
        Assert.NotNull(result);
        Assert.Contains("comments", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SELECT * FROM transactions INTO OUTFILE '/tmp/data.txt'")]
    [InlineData("SELECT * FROM transactions INTO DUMPFILE '/tmp/dump.sql'")]
    [InlineData("select * from transactions into outfile 'file.txt'")]
    public void ValidateSql_RejectsFileExportPatterns(string sql)
    {
        var result = ValidateSql(sql);
        
        Assert.NotNull(result);
        Assert.Contains("export", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SELECT * FROM transactions; DELETE FROM accounts;")]
    [InlineData("SELECT * FROM transactions; DROP TABLE users;")]
    public void ValidateSql_RejectsMultipleStatements(string sql)
    {
        var result = ValidateSql(sql);
        
        Assert.NotNull(result);
        // The validation catches either the forbidden keyword or multiple statements
        Assert.True(
            result.Contains("multiple", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("forbidden", StringComparison.OrdinalIgnoreCase),
            $"Expected error about multiple statements or forbidden operation, got: {result}");
    }

    [Theory]
    [InlineData("DELETE FROM transactions")]
    [InlineData("UPDATE transactions SET amount = 0")]
    [InlineData("INSERT INTO transactions VALUES (...)")]
    [InlineData("DROP TABLE transactions")]
    public void ValidateSql_RejectsForbiddenOperations(string sql)
    {
        var result = ValidateSql(sql);
        
        Assert.NotNull(result);
        // These queries are rejected either because they're not SELECT queries,
        // or because they contain forbidden keywords
        Assert.True(
            result.Contains("forbidden", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("Only SELECT", StringComparison.Ordinal),
            $"Expected error about forbidden operation or non-SELECT query, got: {result}");
    }

    [Theory]
    [InlineData("SELECT * FROM transactions WHERE amount > 0")]
    [InlineData("SELECT COUNT(*) FROM transactions")]
    [InlineData("SELECT * FROM transactions WHERE date >= CURRENT_DATE - INTERVAL '1 month'")]
    [InlineData("SELECT SUM(amount) FROM transactions WHERE amount < 0")]
    [InlineData("SELECT * FROM transactions;")]
    public void ValidateSql_AcceptsValidQueries(string sql)
    {
        var result = ValidateSql(sql);
        
        Assert.Null(result);
    }

    [Fact]
    public void ValidateSql_RejectsEmptyQuery()
    {
        var result = ValidateSql("");
        
        Assert.NotNull(result);
        Assert.Contains("No SQL query", result);
    }

    [Fact]
    public void ValidateSql_RejectsNonSelectQuery()
    {
        var result = ValidateSql("SHOW TABLES");
        
        Assert.NotNull(result);
        Assert.Contains("Only SELECT", result);
    }
}
