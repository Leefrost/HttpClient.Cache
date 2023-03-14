using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.Tests;

public class TestClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow + TimeSpan.FromDays(1);
}