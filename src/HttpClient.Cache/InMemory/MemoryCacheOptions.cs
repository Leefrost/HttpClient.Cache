using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.InMemory;

/// <summary>
/// Provides memory cache configuration
/// </summary>
public class MemoryCacheOptions
{
    /// <summary>
    /// Time frequency to check the cache entries expiration. Default value is 1 minute.
    /// </summary>
    public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1.0);

    /// <summary>
    /// Internal system clock. Default value is <see cref="DefaultSystemClock"/> clock
    /// </summary>
    public ISystemClock Clock { get; set; } = new DefaultSystemClock();
}