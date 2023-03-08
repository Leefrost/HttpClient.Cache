using System.Net;
using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.Stats;

namespace HttpClient.Cache.Tests.Stats;

public class DefaultStatsProviderTests
{
    [Fact]
    public void GetReport_GetDefaultEmptyReportIfNoActions_ReportSuccessful()
    {
        DefaultCacheStatsProvider provider = new("test-cache");
        var expected = new CacheStatsReport("test-cache");

        var stats = provider.GetReport();

        stats.Should().BeEquivalentTo(expected, ignore => ignore.Excluding(x => x.CreatedAt));
    }
    
    [Fact]
    public void ReportHit_ReportCacheHitWith201_ReportSuccessful()
    {
        DefaultCacheStatsProvider provider = new("test-cache");
        var expected = new CacheStatsReport("test-cache")
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
    
    [Fact]
    public void ReportHit_ReportCacheMissWith503_ReportSuccessful()
    {
        DefaultCacheStatsProvider provider = new("test-cache");
        var expected = new CacheStatsReport("test-cache")
        {
            PerStatusCode = new Dictionary<HttpStatusCode, CacheStatsResult>
            {
                { HttpStatusCode.ServiceUnavailable, new CacheStatsResult { CacheHit = 0L, CacheMiss = 1L } }
            }
        };

        provider.ReportMiss(HttpStatusCode.ServiceUnavailable);
        var stats = provider.GetReport();

        stats.Should().BeEquivalentTo(expected, ignore => ignore.Excluding(x => x.CreatedAt));
    }
    
    [Fact]
    public void GetReport_ReportOneMissAndHit_ReportTotalIsSuccessful()
    {
        DefaultCacheStatsProvider provider = new("test-cache");

        provider.ReportHit(HttpStatusCode.Created);
        provider.ReportMiss(HttpStatusCode.Created);
        var stats = provider.GetReport();

        using (new AssertionScope())
        {
            stats.Total.CacheMiss.Should().Be(1L);
            stats.Total.CacheHit.Should().Be(1L);
            stats.Total.TotalRequests.Should().Be(2L);
            stats.Total.TotalHitsPercent.Should().Be(0.5);
            stats.Total.TotalMissPercent.Should().Be(0.5);
        }
    }
}