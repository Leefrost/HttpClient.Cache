using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.InMemory;

namespace HttpClient.Cache.Tests.InMemory;

public class MemoryCacheTests
{
    [Fact]
    public void CreateEntry_Create10NewEntryAndSetValue_CacheContains10Items()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new MemoryCacheOptions();
        var cache = new MemoryCache(options);

        for (var i = 1; i <= 10; ++i)
        {
            using var entry = cache.CreateEntry($"{i}");
            entry.AbsoluteExpirationRelativeToNow = expiration;
            entry.Value = $"value-{i}";
        }

        using (new AssertionScope())
        {
            cache.Count.Should().Be(10);

            var val = cache.TryGetValue("9", out var cachedItem);
            val.Should().BeTrue();
            cachedItem.Should().Be("value-9");
        }
    }
    
    [Fact]
    public async Task CreateEntry_ExpireAbsoluteExpirationRelativeToNow_CacheIsEmpty()
    {
        var expiration = TimeSpan.FromSeconds(3);
        var options = new MemoryCacheOptions();
        var cache = new MemoryCache(options);
        
        using(var entry = cache.CreateEntry("key")){
            entry.AbsoluteExpirationRelativeToNow = expiration;
            entry.Value = $"value";
        }

        using (new AssertionScope())
        {
            cache.Count.Should().Be(1);
            await Task.Delay(expiration);

            var value = cache.TryGetValue("key", out var cacheEntry);
            value.Should().BeFalse();
            cacheEntry.Should().BeNull();
            cache.Count.Should().Be(0);
        }
    }
    
    [Fact]
    public async Task CreateEntry_ExpireSlidingExpiration_CacheIsEmpty()
    {
        var expiration = TimeSpan.FromSeconds(2);
        var options = new MemoryCacheOptions();
        var cache = new MemoryCache(options);
        
        using(var entry = cache.CreateEntry("key")){
            entry.SlidingExpiration = expiration;
            entry.Value = $"value";
        }

        using (new AssertionScope())
        {
            cache.Count.Should().Be(1);
            await Task.Delay(expiration);

            var value = cache.TryGetValue("key", out var cacheEntry);
            value.Should().BeFalse();
            cacheEntry.Should().BeNull();
            cache.Count.Should().Be(0);
        }
    }
    
    [Fact]
    public async Task CreateEntry_ExpireAbsoluteDate_CacheIsEmpty()
    {
        var expiration = DateTimeOffset.UtcNow.AddSeconds(2);
        var options = new MemoryCacheOptions();
        var cache = new MemoryCache(options);
        
        using(var entry = cache.CreateEntry("key")){
            entry.AbsoluteExpiration = expiration;
            entry.Value = $"value";
        }

        using (new AssertionScope())
        {
            cache.Count.Should().Be(1);
            await Task.Delay(TimeSpan.FromSeconds(2));

            var value = cache.TryGetValue("key", out var cacheEntry);
            value.Should().BeFalse();
            cacheEntry.Should().BeNull();
            cache.Count.Should().Be(0);
        }
    }
}