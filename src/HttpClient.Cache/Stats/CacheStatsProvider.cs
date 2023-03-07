using System.Collections.Concurrent;
using System.Net;

namespace HttpClient.Cache.Stats;

public class CacheStatsProvider : ICacheStatsProvider
{
    private readonly string _cacheType;
    private readonly ConcurrentDictionary<HttpStatusCode, CacheStatsResult> _values;

    public CacheStatsProvider(string cacheType)
    {
        _cacheType = cacheType;
        _values = new ConcurrentDictionary<HttpStatusCode, CacheStatsResult>();
    }

    public void ReportHit(HttpStatusCode code)
    {
        _values.AddOrUpdate(code, _ => new CacheStatsResult { CacheHit = 1 }, (_, existing) =>
        {
            existing.CacheHit++;
            return existing;
        });
    }

    public void ReportMiss(HttpStatusCode code)
    {
        _values.AddOrUpdate(code, _ => new CacheStatsResult { CacheMiss = 1 }, (_, existing) =>
        {
            existing.CacheMiss++;
            return existing;
        });
    }

    public StatsReport GetReport()
    {
        return new StatsReport(_cacheType)
        {
            PerStatusCode = new Dictionary<HttpStatusCode, CacheStatsResult>(_values)
        };
    }
}