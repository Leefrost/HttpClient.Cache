using System.Net;
using FluentAssertions;
using HttpClient.Cache.Stats;

namespace HttpClient.Cache.Tests.Stats;

public class StatsProviderTests
{
    [Fact]
    public void ReportHit_ReportCacheHitWith201_ReportSuccessful()
    {
        CacheStatsProvider provider = new("test-cache");
        var expected = new StatsReport("test-cache")
        {
            PerStatusCode = new Dictionary<HttpStatusCode, CacheStatsResult>
            {
                { HttpStatusCode.Created, new CacheStatsResult { CacheHit = 1L, CacheMiss = 0L } }
            }
        };

        provider.ReportHit(HttpStatusCode.Created);
        var stats = provider.GetReport();

        stats.Should().BeEquivalentTo(expected, ignore => ignore.Excluding(x => x.CreatedAt));
    }
}