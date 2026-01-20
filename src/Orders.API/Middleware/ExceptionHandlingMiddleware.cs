using System.Text.Json;
using FluentValidation;
using Orders.Domain.Exceptions;

namespace Orders.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message, details) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "One or more validation errors occurred",
                validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToArray() as object),

            EntityNotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                notFoundException.Code,
                notFoundException.Message,
                null as object),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                domainException.Code,
                domainException.Message,
                null as object),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "You are not authorized to perform this action",
                null as object),

            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred",
                null as object)
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            error = new
            {
                code = errorCode,
                message,
                details,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            }
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
