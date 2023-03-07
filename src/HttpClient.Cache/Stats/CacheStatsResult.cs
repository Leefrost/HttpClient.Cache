namespace HttpClient.Cache.Stats;

public class CacheStatsResult
{
    public long CacheHit { get; set; }
    public long CacheMiss { get; set; }
    public long TotalRequests => CacheHit + CacheMiss;

    public double TotalHitsPercent => CacheHit * 1.0 / TotalRequests;
    
    public double TotalMissPercent => CacheMiss * 1.0 / TotalRequests;
}