## HttpClient.Cache

A lightweight in-memory cache for HttpClient.

### The Purpose

Working with high-load systems or with a system where it is important to have a good response time - the cache is a first citizen. This package contains lightweight, self-written, in-memory cache implementation to catch and store responses based on their status code. The configuration is pretty flexible and gives opportynity to set pair between cache time and response code.

### How to install

```shell
dotnet add package Leefrost.HttpClient.Cache
```

### Plans and TODOs:

- [x] in-memory caching support
- [ ] distributed caching support

### How to use

The code below caches 3 top-kind responses (OK, BadRequest, and InternalServerError) for a different time - 60/10/5 seconds.
HttpClient will do 5 requests to the `https://randomuser.me/api/` and do cache the responses for us.

The cache report will show us 1 miss (initial request) and 4 hits (so the response time will be 0)

```csharp
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
using (var httpClient = new HttpClient(cacheHandler))
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
Console.WriteLine($"--> Hit: {stats.Total.CacheHit} [{stats.Total.TotalHitsPercent}]");
Console.WriteLine($"--> Miss: {stats.Total.CacheMiss} [{stats.Total.TotalMissPercent}]");
Console.ReadLine();
```

Console will show next output:

```shell
Attempt 1: https://randomuser.me/api/ --> OK Done in: 681 ms
Attempt 2: https://randomuser.me/api/ --> OK Done in: 75 ms
Attempt 3: https://randomuser.me/api/ --> OK Done in: 0 ms
Attempt 4: https://randomuser.me/api/ --> OK Done in: 0 ms
Attempt 5: https://randomuser.me/api/ --> OK Done in: 0 ms

Cache stats - total requests: 5
--> Hits: 4 [0,8]
--> Misses: 1 [0,2]

```