using System.Diagnostics;

namespace HttpClient.Cache.InMemory;

internal static class MemoryCacheExtensions
{
    public static Task<bool> TryGetAsync(this IMemoryCache cache, string key, out CacheData? cacheData)
    {
        cacheData = default;
        try
        {
            if (!cache.TryGetValue(key, out var data))
            {
                return Task.FromResult(false);
            }

            if (data is null)
            {
                return Task.FromResult(false);
            }

            var binaryData = (byte[])data;
            cacheData = binaryData.Unpack();
                
            return Task.FromResult(true);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(false);
        }
    }

    public static Task<bool> TrySetAsync(this IMemoryCache cache, string key, CacheData cacheData,
        TimeSpan absoluteExpirationRelativeToNow)
    {
        try
        {
            using (var entry = cache.CreateEntry(key))
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                entry.Value = cacheData.Pack();
            }
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(false);
        }
    }
    
    public static Task<bool> TrySetAsync(this IMemoryCache cache, string key, CacheData cacheData,
        DateTimeOffset absoluteExpiration)
    {
        try
        {
            using (var entry = cache.CreateEntry(key))
            {
                entry.AbsoluteExpiration = absoluteExpiration;
                entry.Value = cacheData.Pack();
            }
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(false);
        }
    }
    
    public static Task<bool> TrySetAsync(this IMemoryCache cache, string key, CacheData cacheData, CacheEntryPriority priority,
        TimeSpan? slidingExpiration)
    {
        try
        {
            using (var entry = cache.CreateEntry(key))
            {
                entry.Priority = priority;
                entry.SlidingExpiration = slidingExpiration;
                entry.Value = cacheData.Pack();
            }
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(false);
        }
    }
}