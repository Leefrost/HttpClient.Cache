using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.InMemory;

public class MemoryCacheOptions
{
    public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1.0);

    public ISystemClock Clock { get; set; } = new SystemClock();
}