using System.Diagnostics;

namespace FinTrack.Host.Middleware;

/// <summary>
/// Middleware that ensures each request has a correlation ID for distributed tracing.
/// The correlation ID is read from the X-Correlation-ID header if present, otherwise generated.
/// It is added to the response headers and logging scope for all downstream processing.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from header or generate a new one
        var correlationId = GetOrCreateCorrelationId(context);

        // Set the correlation ID on the Activity for OpenTelemetry
        Activity.Current?.SetBaggage("CorrelationId", correlationId);

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Add correlation ID to logging scope for all downstream logging
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Check if correlation ID was provided in request header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            var value = correlationId.ToString();
            
            // Validate correlation ID: must be reasonable length and alphanumeric/dash/underscore
            if (value.Length <= 128 && IsValidCorrelationId(value))
            {
                return value;
            }
        }

        // Use trace ID from Activity if available (OpenTelemetry)
        if (Activity.Current?.TraceId.ToString() is { } traceId && !string.IsNullOrEmpty(traceId))
        {
            return traceId;
        }

        // Fall back to generating a new GUID
        return Guid.NewGuid().ToString("N");
    }

    private static bool IsValidCorrelationId(string value)
    {
        // Allow alphanumeric characters, dashes, and underscores (common in trace IDs and GUIDs)
        return value.All(static c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
