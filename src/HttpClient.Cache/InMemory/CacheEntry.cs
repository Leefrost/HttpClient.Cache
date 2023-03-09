using System.Diagnostics;

namespace HttpClient.Cache.InMemory;

internal class CacheEntry : ICacheEntry
{
    private readonly object _lock = new();

    private TimeSpan? _slidingExpiration;
    private DateTimeOffset? _absoluteExpiration;
    private TimeSpan? _absoluteExpirationRelativeToNow;
    
    private IList<IDisposable> _expirationTokenRegistrations = default!;
    private IList<IChangeToken> _expirationTokens = default!;
    private IList<PostEvictionCallbackRegistration> _postEvictionCallbacks = default!;
    
    private readonly Action<CacheEntry> _notifyCacheEntryDisposed;
    private readonly Action<CacheEntry> _notifyCacheOfExpiration;

    private static readonly Action<object> ExpirationCallback = ExpirationTokensExpired;
    
    private bool _isAdded;
    private bool _isExpired;

    internal CacheEntry(object key, Action<CacheEntry> notifyCacheEntryDisposed,
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

    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

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

    public IList<IChangeToken> ExpirationTokens => _expirationTokens;

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => _postEvictionCallbacks;

    internal DateTimeOffset LastAccessed { get; set; }
    internal EvictionReason EvictionReason { get; private set; }

    internal bool IsExpired(DateTimeOffset now)
    {
        if (_isExpired && !CheckForExpiredTime(now))
        {
            return CheckForExpiredTokens();
        }

        return true;
    }

    private bool CheckForExpiredTime(DateTimeOffset now)
    {
        if (_absoluteExpiration.HasValue && _absoluteExpiration.Value <= now)
        {
            SetExpired(EvictionReason.Expired);
            return true;
        }

        if (_slidingExpiration.HasValue)
        {
            TimeSpan timeSpan = now - LastAccessed;
            TimeSpan? slidingWindows = _slidingExpiration;
            if ((slidingWindows.HasValue ? timeSpan >= slidingWindows.GetValueOrDefault() ? 1 : 0 : 0) != 0)
            {
                SetExpired(EvictionReason.Expired);
                return true;
            }
        }

        return false;
    }

    internal void SetExpired(EvictionReason reason)
    {
        if (EvictionReason == null)
        {
            EvictionReason = reason;
        }

        _isExpired = true;
        DetachTokens();
    }

    internal bool CheckForExpiredTokens()
    {
        if (_expirationTokens != null)
        {
            for (int index = 0; index < _expirationTokens.Count; ++index)
            {
                if (_expirationTokens[index].HasChanged)
                {
                    SetExpired(EvictionReason.TokenExpired);
                    return true;
                }
            }
        }

        return false;
    }

    internal void AttachTokens()
    {
        if (_expirationTokens == null)
        {
            return;
        }

        lock (_lock)
        {
            for (int index = 0; index < _expirationTokens.Count; ++index)
            {
                IChangeToken token = _expirationTokens[index];
                if (token.ActiveChangeCallbacks)
                {
                    if (_expirationTokenRegistrations == null)
                    {
                        _expirationTokenRegistrations = new List<IDisposable>();
                    }

                    _expirationTokenRegistrations.Add(token.RegisterChangeCallback(ExpirationCallback, this));
                }
            }
        }
    }

    private static void ExpirationTokensExpired(object obj)
    {
        Task.Factory.StartNew(state =>
        {
            CacheEntry? cacheEntry = (CacheEntry)state;
            cacheEntry.SetExpired(EvictionReason.TokenExpired);
            cacheEntry._notifyCacheOfExpiration(cacheEntry);
        }, obj, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }


    internal void DetachTokens()
    {
        lock (_lock)
        {
            IList<IDisposable>? registrations = _expirationTokenRegistrations;
            if (registrations == null)
            {
                return;
            }

            _expirationTokenRegistrations = null;
            foreach (IDisposable registration in registrations)
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

        TaskFactory factory = Task.Factory;
        CancellationToken token = CancellationToken.None;
        TaskScheduler scheduler = TaskScheduler.Default;
        factory.StartNew(state => InvokeCallbacks((CacheEntry)state), this, token, TaskCreationOptions.DenyChildAttach,
            scheduler);
    }

    private static void InvokeCallbacks(CacheEntry entry)
    {
        IList<PostEvictionCallbackRegistration>? callbackRegistrationList =
            Interlocked.Exchange(ref entry._postEvictionCallbacks, null);
        
        if (callbackRegistrationList == null)
        {
            return;
        }

        foreach (PostEvictionCallbackRegistration postEvictionCallbackRegistration in callbackRegistrationList)
        {
            try
            {
                PostEvictionDelegate? evictionCallback = postEvictionCallbackRegistration.EvictionCallback;
                if (evictionCallback != null)
                {
                    object key = entry.Key;
                    object value = entry.Value;
                    EvictionReason reason = entry.EvictionReason;
                    object state = postEvictionCallbackRegistration.State;
                    evictionCallback.Invoke(key, value, reason.ToString(), state);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}");
            }
        }
    }

    internal void PropagateOptions(CacheEntry parent)
    {
        if (parent == null)
        {
            return;
        }

        if (_expirationTokens != null)
        {
            lock (_lock)
            {
                lock (parent._lock)
                {
                    using (IEnumerator<IChangeToken> changeTokenEnumerator = _expirationTokens.GetEnumerator())
                    {
                        while (changeTokenEnumerator.MoveNext())
                        {
                            IChangeToken changeToken = changeTokenEnumerator.Current;
                            parent.AddExpirationToken(changeToken);
                        }
                    }
                }
            }
        }

        if (!_absoluteExpiration.HasValue)
        {
            return;
        }

        if (parent._absoluteExpiration.HasValue)
        {
            DateTimeOffset? expiration = _absoluteExpiration;
            DateTimeOffset? parentExpiration = parent._absoluteExpiration;
            if ((expiration.HasValue & parentExpiration.HasValue
                    ? expiration.GetValueOrDefault() < parentExpiration.GetValueOrDefault() ? 1 : 0
                    : 0) == 0)
            {
                return;
            }
        }

        parent._absoluteExpiration = _absoluteExpiration;
    }
    
    public void Dispose()
    {
        if (_isAdded)
        {
            return;
        }

        _isAdded = true;
        _notifyCacheEntryDisposed(this);
    }
}