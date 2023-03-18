using System.Diagnostics;
using System.Net;
using HttpClient.Cache.InMemory;

const string url = "http://worldclockapi.com/api/json/utc/now";

var cacheExpiration = new Dictionary<HttpStatusCode, TimeSpan>()
{
    {HttpStatusCode.OK, TimeSpan.FromSeconds(60)},
    {HttpStatusCode.BadRequest, TimeSpan.FromSeconds(10)},
    {HttpStatusCode.InternalServerError, TimeSpan.FromSeconds(5)}
};


var httpClientHandler = new HttpClientHandler();
var inMemoryResponseCache = new InMemoryCacheHandler(httpClientHandler, cacheExpiration);
using (var client = new System.Net.Http.HttpClient(inMemoryResponseCache))
{
    for (int i = 0; i < 5; i++)
    {
        Console.Write($"Try: {i}: {url}");
        var stopwatch = Stopwatch.StartNew();

        var result = client.GetAsync(url).GetAwaiter().GetResult();

        var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        Console.WriteLine($"Done in: {stopwatch.ElapsedMilliseconds} ms");
        
        Thread.Sleep(1000);
    }
}

Console.WriteLine("Stats:");

var stats = inMemoryResponseCache.StatsProvider.GetReport();
Console.WriteLine($"Total: {stats.Total.TotalRequests}");
Console.WriteLine($"-> Hit: {stats.Total.CacheHit} [{stats.Total.TotalHitsPercent}]");
Console.WriteLine($"-> Miss: {stats.Total.CacheMiss} [{stats.Total.TotalMissPercent}]");
Console.ReadLine();
