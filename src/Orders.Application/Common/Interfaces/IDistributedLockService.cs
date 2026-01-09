namespace Orders.Application.Common.Interfaces;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
}
