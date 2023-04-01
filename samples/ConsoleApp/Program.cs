using System.Diagnostics;
using System.Net;
using HttpClient.Cache.InMemory;

const string url = "https://randomuser.me/api/";

//Setting the cache time for each required status
var cacheExpiration = new Dictionary<HttpStatusCode, TimeSpan>
{
    {HttpStatusCode.OK, TimeSpan.FromSeconds(60)},
    {HttpStatusCode.BadRequest, TimeSpan.FromSeconds(10)},
    {HttpStatusCode.InternalServerError, TimeSpan.FromSeconds(5)}
};

//Calling the API and cache the responses
var requestHandler = new HttpClientHandler();
var cacheHandler = new InMemoryCacheHandler(requestHandler, cacheExpiration);
using (var httpClient = new System.Net.Http.HttpClient(cacheHandler))
{
    for (int i = 1; i <= 5; ++i)
    {
        Console.Write($"Attempt {i}: {url}");

        var stopwatch = Stopwatch.StartNew();
        var result = await httpClient.GetAsync(url);
        Console.Write($" --> {result.StatusCode} ");
        stopwatch.Stop();
        
        Console.WriteLine($"Done in: {stopwatch.ElapsedMilliseconds} ms");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
    Console.WriteLine();
}

//Checking cache stats
var stats = cacheHandler.StatsProvider.GetReport();
Console.WriteLine($"Cache stats - total requests: {stats.Total.TotalRequests}");
Console.WriteLine($"--> Hits: {stats.Total.CacheHit} [{stats.Total.TotalHitsPercent}]");
Console.WriteLine($"--> Misses: {stats.Total.CacheMiss} [{stats.Total.TotalMissPercent}]");
Console.ReadLine();
