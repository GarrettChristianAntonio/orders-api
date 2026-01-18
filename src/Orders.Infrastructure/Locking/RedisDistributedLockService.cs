using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Orders.Infrastructure.Locking;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLockService> _logger;

    public RedisDistributedLockService(IConnectionMultiplexer redis, ILogger<RedisDistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();

        var acquired = await db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);

        if (acquired)
        {
            _logger.LogDebug("Acquired lock for resource {Resource}", resource);
            return new RedisLock(db, lockKey, lockValue, _logger);
        }

        _logger.LogDebug("Failed to acquire lock for resource {Resource}", resource);
        return null;
    }

    public async Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();

        return await db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
    }

    private sealed class RedisLock : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed;

        public RedisLock(IDatabase db, string lockKey, string lockValue, ILogger logger)
        {
            _db = db;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                var script = @"
                    if redis.call('get', KEYS[1]) == ARGV[1] then
                        return redis.call('del', KEYS[1])
                    else
                        return 0
                    end";

                await _db.ScriptEvaluateAsync(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });
                _logger.LogDebug("Released lock for key {LockKey}", _lockKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error releasing lock for key {LockKey}", _lockKey);
            }
        }
    }
}
