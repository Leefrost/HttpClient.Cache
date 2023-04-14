namespace HttpClient.Cache;

/// <summary>
/// Cache value holder
/// </summary>
public interface ICacheEntry: IDisposable
{
    /// <summary>
    /// Cache value
    /// </summary>
    object Value { get; set; }
    
    /// <summary>
    /// Cache entry priority
    /// </summary>
    CacheEntryPriority Priority { get; set; }
    
    /// <summary>
    /// Entry absolute expiration. The entry will be expired after a set amount of time
    /// </summary>
    DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Entry absolute expiration relative to created time. The entry will be expired a set amount of time from now
    /// </summary>
    TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    
    /// <summary>
    /// Entry sliding expiration. Entry will be expired if it hasn't been accessed in a set amount of time. 
    /// </summary>
    TimeSpan? SlidingExpiration { get; set; }
    
    /// <summary>
    /// Collection of <see cref="IChangeToken"/> tokens.
    /// Any lifecycle changes will appear here
    /// </summary>
    IList<IChangeToken> ExpirationTokens { get; }

    /// <summary>
    /// Collection of <see cref="PostEvictionCallbackRegistration"/> callbacks.
    /// Will be executed on entry eviction
    /// </summary>
    IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
}