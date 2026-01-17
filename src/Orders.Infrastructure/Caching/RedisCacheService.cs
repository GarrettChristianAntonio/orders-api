using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Orders.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cached = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cached))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cached value for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            await _cache.SetStringAsync(key, serialized, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cached value for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cached value for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _redis.GetServers().FirstOrDefault();
            if (server == null)
            {
                return;
            }

            var db = _redis.GetDatabase();
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (keys.Length > 0)
            {
                await db.KeyDeleteAsync(keys);
                _logger.LogDebug("Removed {Count} cached keys with prefix {Prefix}", keys.Length, prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cached values with prefix {Prefix}", prefix);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);

        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }
}
