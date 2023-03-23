# HttpClient.Cache

A caching wrapper around HttpClient to cache responses

### The Purpose

Working with some high load systems or with system where is important to have good response time
cache is a first one citizen.

### Install

//TODO Nuget deployment

```shell
foo@bar:~$ Install-Package HttpClient.Cache
```

### Examples

```csharp
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
        Console.Write($"Try: {i}: {url} ");

        var stopwatch = Stopwatch.StartNew();
        var result = await httpClient.GetAsync(url);
        Console.Write($" --> {result.StatusCode} ");
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
```
Will generate next output:
```console
Try: 1: http://worldclockapi.com/api/json/utc/now  --> OK Done in: 450 ms
Try: 2: http://worldclockapi.com/api/json/utc/now  --> OK Done in: 57 ms
Try: 3: http://worldclockapi.com/api/json/utc/now  --> OK Done in: 0 ms
Try: 4: http://worldclockapi.com/api/json/utc/now  --> OK Done in: 0 ms
Try: 5: http://worldclockapi.com/api/json/utc/now  --> OK Done in: 0 ms
Cache stats - total requests: 5
--> Hit: 4 [0,8]
--> Miss: 1 [0,2]

```