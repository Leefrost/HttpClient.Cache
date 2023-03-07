using System.Net;

namespace HttpClient.Cache.Stats;

public class StatsReport
{
    public StatsReport(string cacheType)
    {
        CacheType = cacheType;
        PerStatusCode = new Dictionary<HttpStatusCode, CacheStatsResult>();
        CreatedAt = DateTimeOffset.Now;
    }

    public DateTimeOffset CreatedAt { get; }

    public string CacheType { get; }

    public Dictionary<HttpStatusCode, CacheStatsResult> PerStatusCode { get; init; }

    public CacheStatsResult Total => new()
    {
        CacheHit = PerStatusCode.Sum(status => status.Value.CacheHit),
        CacheMiss = PerStatusCode.Sum(status => status.Value.CacheMiss)
    };
}