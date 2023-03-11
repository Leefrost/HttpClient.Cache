using System.Diagnostics;

namespace HttpClient.Cache.InMemory;

public static class InMemoryCacheExtensions
{
    public static Task<CacheData> TryGetAsync(this IMemoryCache cache, string key)
    {
        try
        {
            if (cache.TryGetValue(key, out var data))
            {
                var binaryData = (byte[])data;
                return Task.FromResult(binaryData.Deserialize());
            }

            return Task.FromResult(default(CacheData));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(default(CacheData));
        }
    }

    public static Task TrySetAsync(this IMemoryCache cache, string key, CacheData value,
        TimeSpan absoluteExpirationRelativeToNow)
    {
        try
        {
            var entry = cache.CreateEntry(key);
            entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            entry.Value = value.Serialize();
            entry.Dispose();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return Task.FromResult(false);
        }
    }
}