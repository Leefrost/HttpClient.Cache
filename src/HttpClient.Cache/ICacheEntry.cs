namespace HttpClient.Cache;

public interface ICacheEntry: IDisposable
{
    object Value { get; set; }
    
    DateTimeOffset? AbsoluteExpiration { get; set; }

    TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    
    TimeSpan? SlidingExpiration { get; set; }
    
    IList<IChangeToken> ExpirationTokens { get; }

    IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
    
    CacheEntryPriority Priority { get; set; }
}