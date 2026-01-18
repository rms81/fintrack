using System.Diagnostics;

namespace FinTrack.Host.Middleware;

/// <summary>
/// Middleware that logs HTTP request details with structured logging.
/// Provides visibility into request duration, status codes, and key metrics.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check endpoints to reduce noise
        if (IsHealthCheckEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        try
        {
            await _next(context);
            stopwatch.Stop();

            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                : context.Response.StatusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms | RequestId: {RequestId} | User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId,
                context.User.Identity?.Name ?? "anonymous");
        }
        catch (Exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                "HTTP {Method} {Path} threw exception after {ElapsedMs}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                requestId);

            throw;
        }
    }

    private static bool IsHealthCheckEndpoint(PathString path)
    {
        return path.StartsWithSegments("/health")
            || path.StartsWithSegments("/alive")
            || path.StartsWithSegments("/ready");
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
