using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Host.Exceptions;

/// <summary>
/// Middleware that handles exceptions and returns RFC 7807 Problem Details responses.
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, context, traceId),
            AppException appEx => CreateProblemDetails(appEx, context, traceId),
            _ => CreateInternalServerErrorProblemDetails(exception, context, traceId)
        };

        // Log the exception
        if (exception is AppException)
        {
            _logger.LogWarning(
                exception,
                "Application exception occurred. TraceId: {TraceId}, Type: {ExceptionType}, Path: {Path}",
                traceId,
                exception.GetType().Name,
                context.Request.Path);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
                traceId,
                context.Request.Path);
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(AppException exception, HttpContext context, string traceId)
    {
        return new ProblemDetails
        {
            Type = exception.ErrorType,
            Title = GetTitle(exception.StatusCode),
            Status = exception.StatusCode,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        ValidationException exception,
        HttpContext context,
        string traceId)
    {
        return new ValidationProblemDetails(exception.Errors)
        {
            Type = exception.ErrorType,
            Title = "Validation Error",
            Status = exception.StatusCode,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }

    private ProblemDetails CreateInternalServerErrorProblemDetails(
        Exception exception,
        HttpContext context,
        string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://fintrack.app/errors/internal-server-error",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = _environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };

        // Include stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                type = exception.GetType().Name,
                message = exception.Message,
                stackTrace = exception.StackTrace
            };
        }

        return problemDetails;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        _ => "Error"
    };
}

/// <summary>
/// Extension methods for registering the exception handler middleware.
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseAppExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
