using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.InMemory;

public class MemoryCacheOptions
{
    public TimeSpan ExpirationScanFrequency { get; } = TimeSpan.FromMinutes(1.0);
    
    public ISystemClock Clock { get; set; }
}