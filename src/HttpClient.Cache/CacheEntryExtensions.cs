namespace HttpClient.Cache;

public static class CacheEntryExtensions
{
    public static ICacheEntry SetValue(this ICacheEntry entry, object value)
    {
        entry.Value = value;
        return entry;
    }
    
    public static ICacheEntry SetPriority(this ICacheEntry entry, CacheEntryPriority priority)
    {
        entry.Priority = priority;
        return entry;
    }

    public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, DateTimeOffset expiredAt)
    {
        entry.AbsoluteExpiration = expiredAt;
        return entry;
    }

    public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, TimeSpan expiredAt)
    {
        entry.AbsoluteExpirationRelativeToNow = expiredAt;
        return entry;
    }

    public static ICacheEntry SetSlidingExpiration(this ICacheEntry entry, TimeSpan slidingExpiration)
    {
        entry.SlidingExpiration = slidingExpiration;
        return entry;
    }
    
    public static ICacheEntry AddExpirationToken(this ICacheEntry entry, IChangeToken token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }
        
        entry.ExpirationTokens.Add(token);
        return entry;
    }

    public static ICacheEntry RegisterPostEvictionCallback(this ICacheEntry entry, PostEvictionDelegate callback,
        object state)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }
        
        entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration(callback, state));
        return entry;
    }
}