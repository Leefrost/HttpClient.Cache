using System.Net;

namespace HttpClient.Cache.Stats;

public interface ICacheStatsProvider
{
    void ReportHit(HttpStatusCode code);

    void ReportMiss(HttpStatusCode code);

    StatsReport GetReport();
}