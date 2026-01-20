using System.Text.Json;
using Orders.Application.Common.Interfaces;
using Orders.Infrastructure.Caching;

namespace Orders.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    private const string IdempotencyKeyHeader = "X-Idempotency-Key";
    private static readonly TimeSpan IdempotencyExpiration = TimeSpan.FromHours(24);

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cacheKey = CacheKeys.Idempotency(idempotencyKey!);
        var cachedResponse = await cacheService.GetAsync<IdempotencyResponse>(cacheKey);

        if (cachedResponse != null)
        {
            _logger.LogInformation(
                "Returning cached response for idempotency key {IdempotencyKey}",
                idempotencyKey.ToString());

            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = "application/json";

            foreach (var header in cachedResponse.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }

            await context.Response.WriteAsync(cachedResponse.Body);
            return;
        }

        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var idempotencyResponse = new IdempotencyResponse
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBodyText,
                Headers = context.Response.Headers
                    .Where(h => !h.Key.StartsWith("Transfer-") && h.Key != "Content-Length")
                    .ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            await cacheService.SetAsync(cacheKey, idempotencyResponse, IdempotencyExpiration);

            _logger.LogDebug(
                "Cached response for idempotency key {IdempotencyKey}",
                idempotencyKey.ToString());
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private sealed class IdempotencyResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = [];
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IdempotencyMiddleware>();
    }
}
