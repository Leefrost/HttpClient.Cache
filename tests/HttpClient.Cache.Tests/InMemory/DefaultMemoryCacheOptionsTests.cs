using FluentAssertions;
using HttpClient.Cache.InMemory;
using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.Tests.InMemory;

public class DefaultMemoryCacheOptionsTests
{
    [Fact]
    public void Default_CheckDefaultOptionsForMemoryCache_ReturnExpectedConfiguration()
    {
        var expectedScanFrequency = TimeSpan.FromMinutes(1.0);

        var options = new MemoryCacheOptions();

        options.ExpirationScanFrequency.Should().Be(expectedScanFrequency);
        options.Clock.Should().NotBeNull().And.BeOfType<DefaultSystemClock>();
    }
}