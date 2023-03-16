using FluentAssertions;
using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.Tests.InMemory.Clock;

public class SystemClockTests
{
    [Fact]
    public void GetTime_CheckCurrentTime_ReturnUtcNow()
    {
        var systemClock = new SystemClock();

        systemClock.UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}