using System.Diagnostics;
using System.Net;
using HttpClient.Cache.InMemory;

const string url = "http://worldclockapi.com/api/json/utc/now";

//Set the cache time for each required status
var cacheExpiration = new Dictionary<HttpStatusCode, TimeSpan>
{
    {HttpStatusCode.OK, TimeSpan.FromSeconds(60)},
    {HttpStatusCode.BadRequest, TimeSpan.FromSeconds(10)},
    {HttpStatusCode.InternalServerError, TimeSpan.FromSeconds(5)}
};

//Client calls API and caches it
//Report will show 1 Miss (initial) and 4 Hits. 
var innerHandler = new HttpClientHandler();
var cacheHandler = new InMemoryCacheHandler(innerHandler, cacheExpiration);
using (var httpClient = new System.Net.Http.HttpClient(cacheHandler))
{
    for (int i = 1; i <= 5; ++i)
    {
        Console.Write($"Try: {i}: {url}");

        var stopwatch = Stopwatch.StartNew();
        await httpClient.GetAsync(url);
        stopwatch.Stop();
        
        Console.WriteLine($"Done in: {stopwatch.ElapsedMilliseconds} ms");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}

var stats = cacheHandler.StatsProvider.GetReport();
Console.WriteLine($"Cache stats - total requests: {stats.Total.TotalRequests}");
Console.WriteLine($"--> Hit: {stats.Total.CacheHit} [{stats.Total.TotalHitsPercent}]");
Console.WriteLine($"--> Miss: {stats.Total.CacheMiss} [{stats.Total.TotalMissPercent}]");
Console.ReadLine();
