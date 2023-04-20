namespace HttpClient.Cache.InMemory;

public static class MemoryCacheEntryExtensions
{
    public static ICacheEntry SetOptions(this ICacheEntry entry, MemoryCacheEntryOptions options)
    {
        entry.AbsoluteExpiration = options.AbsoluteExpiration;
        entry.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        entry.SlidingExpiration = options.SlidingExpiration;
        entry.Priority = options.Priority;

        foreach (var expirationToken in options.ExpirationTokens)
        {
            entry.ExpirationTokens.Add(expirationToken);
        }
        
        foreach (var evictionCallback in options.PostEvictionCallbacks)
        {
            entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration(evictionCallback.EvictionCallback, evictionCallback.State));
        }
        
        return entry;
    }
}