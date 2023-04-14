using System.Net;

namespace HttpClient.Cache.Stats;

/// <summary>
/// Defines a mechanism to report cache hit/miss and generate on this info a cache report
/// </summary>
public interface ICacheStatsProvider
{
    /// <summary>
    /// Mark a cache hit
    /// </summary>
    /// <param name="code">Target code</param>
    void ReportHit(HttpStatusCode code);

    /// <summary>
    /// Mark a cache miss
    /// </summary>
    /// <param name="code">Target code</param>
    void ReportMiss(HttpStatusCode code);

    /// <summary>
    /// Gets stored cache report
    /// </summary>
    /// <returns><see cref="CacheStatsReport"/> report</returns>
    CacheStatsReport GetReport();
}