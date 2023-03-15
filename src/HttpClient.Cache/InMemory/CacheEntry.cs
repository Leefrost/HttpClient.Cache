using System.Diagnostics;

namespace HttpClient.Cache.InMemory;

internal class CacheEntry : ICacheEntry
{
    private readonly object _lock = new();
    private static readonly Action<object> ExpirationCallback = ExpirationTokensExpired;

    private readonly Action<CacheEntry> _notifyCacheEntryDisposed;
    private readonly Action<CacheEntry> _notifyCacheOfExpiration;
    private DateTimeOffset? _absoluteExpiration;
    private TimeSpan? _absoluteExpirationRelativeToNow;
    private TimeSpan? _slidingExpiration;

    private IList<IDisposable>? _expirationTokenRegistrations;
    private IList<IChangeToken>? _expirationTokens;
    private IList<PostEvictionCallbackRegistration>? _postEvictionCallbacks;

    private bool _isDisposed;
    private bool _isExpired;

    internal CacheEntry(
        object key, 
        Action<CacheEntry> notifyCacheEntryDisposed,
        Action<CacheEntry> notifyCacheOfExpiration)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));

        _notifyCacheEntryDisposed = notifyCacheEntryDisposed ??
                                    throw new ArgumentNullException(nameof(notifyCacheEntryDisposed));
        _notifyCacheOfExpiration =
            notifyCacheOfExpiration ?? throw new ArgumentNullException(nameof(notifyCacheOfExpiration));
    }

    public object Key { get; }
    public object Value { get; set; }
    
    public CacheEntryPriority Priority { get; set; } = CacheEntryPriority.Normal;

    public DateTimeOffset? AbsoluteExpiration
    {
        get => _absoluteExpiration;
        set => _absoluteExpiration = value;
    }

    public TimeSpan? AbsoluteExpirationRelativeToNow
    {
        get => _absoluteExpirationRelativeToNow;
        set
        {
            if ((value.HasValue ? value.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0 : 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(AbsoluteExpirationRelativeToNow), value,
                    "The relative expiration must be positive");
            }

            _absoluteExpirationRelativeToNow = value;
        }
    }

    public TimeSpan? SlidingExpiration
    {
        get => _slidingExpiration;
        set
        {
            if ((value.HasValue ? value.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0 : 0) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SlidingExpiration), value,
                    "The sliding expiration must be positive");
            }

            _slidingExpiration = value;
        }
    }

    public IList<IChangeToken> ExpirationTokens
    {
        get
        {
            return _expirationTokens ??= new List<IChangeToken>();
        }
    }

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => 
        _postEvictionCallbacks ?? new List<PostEvictionCallbackRegistration>();
    
    internal DateTimeOffset LastAccessed { get; set; }
    
    internal EvictionReason EvictionReason { get; private set; }
    
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _notifyCacheEntryDisposed(this);
    }
    
    internal bool IsExpired(DateTimeOffset currentTime)
    {
        if (!_isExpired && !IsExpiredNow(currentTime))
        {
            return IsAnyExpirationTokenExpired();
        }
        return true;
    }

    private bool IsExpiredNow(DateTimeOffset currentTime)
    {
        if (_absoluteExpiration.HasValue && _absoluteExpiration.Value <= currentTime)
        {
            ExpireEntryByReason(EvictionReason.Expired);
            return true;
        }

        if (!_slidingExpiration.HasValue)
        {
            return false;
        }

        var accessedOffset = currentTime - LastAccessed;
        var expiration = _slidingExpiration;
        if ((expiration.HasValue ? (accessedOffset >= expiration.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        {
            ExpireEntryByReason(EvictionReason.Expired);
            return true;
        }
        
        return false;
    }
    
    private bool IsAnyExpirationTokenExpired()
    {
        if (_expirationTokens == null)
        {
            return false;
        }

        foreach (var token in _expirationTokens)
        {
            if (!token.HasChanged)
            {
                continue;
            }

            ExpireEntryByReason(EvictionReason.TokenExpired);
            return true;
        }

        return false;
    }
    
    private static void ExpirationTokensExpired(object entry)
    {
        Task.Factory.StartNew(state =>
        {
            CacheEntry? cacheEntry = state as CacheEntry;
            cacheEntry?.ExpireEntryByReason(EvictionReason.TokenExpired);
            cacheEntry?._notifyCacheOfExpiration(cacheEntry);
        }, entry, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    internal void ExpireEntryByReason(EvictionReason reason)
    {
        if (EvictionReason == EvictionReason.None)
        {
            EvictionReason = reason;
        }

        _isExpired = true;
        DetachTokens();
    }

    internal void AttachTokens()
    {
        if (_expirationTokens == null)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var token in _expirationTokens)
            {
                if (!token.ActiveChangeCallbacks)
                {
                    continue;
                }

                _expirationTokenRegistrations ??= new List<IDisposable>();
                _expirationTokenRegistrations.Add(token.RegisterChangeCallback(ExpirationCallback, this));
            }
        }
    }
    
    internal void DetachTokens()
    {
        lock (_lock)
        {
            var tokenRegistrations = _expirationTokenRegistrations;
            if (tokenRegistrations == null)
            {
                return;
            }

            _expirationTokenRegistrations = null;
            foreach (IDisposable registration in tokenRegistrations)
            {
                registration.Dispose();
            }
        }
    }
    
    internal void InvokeEvictionCallbacks()
    {
        if (_postEvictionCallbacks == null)
        {
            return;
        }

        Task.Factory.StartNew(state => InvokeCallbacks((CacheEntry)state), this, CancellationToken.None,
            TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    private static void InvokeCallbacks(CacheEntry entry)
    {
        var evictionCallbacks =
            Interlocked.Exchange(ref entry._postEvictionCallbacks, null);

        if (evictionCallbacks == null)
        {
            return;
        }

        foreach (var callback in evictionCallbacks)
        {
            try
            {
                var evictionCallback = callback.EvictionCallback;
                if (evictionCallback == null)
                {
                    continue;
                }

                object key = entry.Key;
                object? value = entry.Value;
                EvictionReason reason = entry.EvictionReason;
                object? state = callback.State;
                
                evictionCallback.Invoke(key, value, reason.ToString(), state);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }
    }
}