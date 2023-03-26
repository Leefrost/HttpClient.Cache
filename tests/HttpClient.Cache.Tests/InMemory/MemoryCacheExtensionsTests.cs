using System.Net;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.InMemory;

namespace HttpClient.Cache.Tests.InMemory;

public class MemoryCacheExtensionsTests
{
    [Fact]
    public async Task  TryGetAsync_GetCacheItemFromMemoryCacheOut_ReturnTrue()
    {
        const string cacheKey = "key";
        var headers = new Dictionary<string, IEnumerable<string>> { { "header", new[] { "header-value" } } };
        var contentHeaders = new Dictionary<string, IEnumerable<string>> { { "contentHeader", new[] { "contentHeader-value" } } };
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheData = new CacheData(Encoding.UTF8.GetBytes("message"), headers, contentHeaders,
            new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        
        using (var entry = memoryCache.CreateEntry(cacheKey))
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            entry.Value = cacheData.Pack();
        }
        
        var result = await memoryCache.TryGetAsync(cacheKey, out var packedData);

        using (new AssertionScope())
        {
            result.Should().BeTrue();
            packedData.Should().BeEquivalentTo(cacheData);
        }
    }
    
    [Fact]
    public async Task  TrySetAsync_SetCacheDataByKeyAndExpRelativeToNow_ReturnTrue()
    {
        const string cacheKey = "key";
        var headers = new Dictionary<string, IEnumerable<string>> { { "header", new[] { "header-value" } } };
        var contentHeaders = new Dictionary<string, IEnumerable<string>> { { "contentHeader", new[] { "contentHeader-value" } } };
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheData = new CacheData(Encoding.UTF8.GetBytes("message"), headers, contentHeaders,
            new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var absoluteTimeoutRelativeToNow = TimeSpan.FromDays(1);
        
        var result = await memoryCache.TrySetAsync(cacheKey, cacheData, absoluteTimeoutRelativeToNow);

        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task  TrySetAsync_SetCacheDataByKeyAndAbsoluteExp_ReturnTrue()
    {
        const string cacheKey = "key";
        var headers = new Dictionary<string, IEnumerable<string>> { { "header", new[] { "header-value" } } };
        var contentHeaders = new Dictionary<string, IEnumerable<string>> { { "contentHeader", new[] { "contentHeader-value" } } };
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheData = new CacheData(Encoding.UTF8.GetBytes("message"), headers, contentHeaders,
            new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var absoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);
        
        var result = await memoryCache.TrySetAsync(cacheKey, cacheData, absoluteExpiration);

        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task  TrySetAsync_SetCacheDataByKeySlidingWindow_ReturnTrue()
    {
        const string cacheKey = "key";
        var headers = new Dictionary<string, IEnumerable<string>> { { "header", new[] { "header-value" } } };
        var contentHeaders = new Dictionary<string, IEnumerable<string>> { { "contentHeader", new[] { "contentHeader-value" } } };
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheData = new CacheData(Encoding.UTF8.GetBytes("message"), headers, contentHeaders,
            new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var slidingExpiration = TimeSpan.FromDays(1);
        
        var result = await memoryCache.TrySetAsync(cacheKey, cacheData, CacheEntryPriority.Normal, slidingExpiration);

        result.Should().BeTrue();
    }
}