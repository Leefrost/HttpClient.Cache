namespace HttpClient.Cache.InMemory;

public class MemoryCacheEntryOptions
{
    private TimeSpan? _absoluteExpirationRelativeToNow;

    private TimeSpan? _slidingExpiration;

    private DateTimeOffset? _absoluteExpiration;

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
    
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;
    
    public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } =
        new List<PostEvictionCallbackRegistration>();
}