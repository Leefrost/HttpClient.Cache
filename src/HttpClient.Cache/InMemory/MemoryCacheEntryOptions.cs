namespace HttpClient.Cache.InMemory;

/// <summary>
/// Provides cache entry configuration
/// </summary>
public class MemoryCacheEntryOptions
{
    private TimeSpan? _absoluteExpirationRelativeToNow;

    private TimeSpan? _slidingExpiration;

    private DateTimeOffset? _absoluteExpiration;

    /// <summary>
    /// The absolute expiration for cache entry. The point in time where cache entry will no longer be available
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Time point must not be in the past</exception>
    public DateTimeOffset? AbsoluteExpiration
    {
        get { return _absoluteExpiration; }
        set
        {
            if (value.HasValue && value.Value < DateTimeOffset.Now)
                throw new ArgumentOutOfRangeException(nameof(AbsoluteExpiration), value,
                    "The absolute expiration can not be in the past");

            _absoluteExpiration = value;
        }
    }

    /// <summary>
    /// The absolute expiration due to now. The time period while entity will be available from now.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Time point should be positive and bigger to <see cref="TimeSpan.Zero"/> value</exception>
    public TimeSpan? AbsoluteExpirationRelativeToNow
    {
        get
        {
            return _absoluteExpirationRelativeToNow;
        }
        set
        {
            if ((value.HasValue ? (value.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0) : 0) != 0)
                throw new ArgumentOutOfRangeException(nameof(AbsoluteExpirationRelativeToNow), value,
                    "Relative expiration must be a positive");

            _absoluteExpirationRelativeToNow = value;
        }
    }

    /// <summary>
    /// Cache entry sliding expiration. The un-active time for cache entry. Do not extends the <see cref="AbsoluteExpiration"/> if it set.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Time point should be positive and bigger to <see cref="TimeSpan.Zero"/> value</exception>
    public TimeSpan? SlidingExpiration
    {
        get { return _slidingExpiration; }
        set
        {
            if ((value.HasValue ? (value.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0) : 0) != 0)
                throw new ArgumentOutOfRangeException(nameof(SlidingExpiration), value,
                    "Sliding expiration must be positive");

            _slidingExpiration = value;
        }
    }
    
    /// <summary>
    /// The cache entry priority. Default value is <see cref="CacheEntryPriority.Normal"/>
    /// </summary>
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;
    
    /// <summary>
    /// The collection of <see cref="IChangeToken"/> expiration tokens
    /// </summary>
    public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

    /// <summary>
    /// The collection of <see cref="PostEvictionCallbackRegistration"/> eviction callbacks
    /// </summary>
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } =
        new List<PostEvictionCallbackRegistration>();
}