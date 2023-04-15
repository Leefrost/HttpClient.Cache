using System.Collections.Concurrent;
using HttpClient.Cache.InMemory.Clock;

namespace HttpClient.Cache.InMemory;

public sealed class MemoryCache : IMemoryCache
{
    private readonly ISystemClock _clock;
    private readonly ConcurrentDictionary<object, MemoryCacheEntry> _cacheEntries;
    private readonly Action<MemoryCacheEntry> _entryExpirationNotification;
    
    private readonly TimeSpan _expirationScanFrequency;
    private readonly Action<MemoryCacheEntry> _setEntry;

    private bool _isDisposed;
    private DateTimeOffset _lastExpirationScan;
    
    private ICollection<KeyValuePair<object, MemoryCacheEntry>> CacheEntries => _cacheEntries;

    public MemoryCache() 
        : this(new MemoryCacheOptions())
    {
    }

    public MemoryCache(MemoryCacheOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _cacheEntries = new ConcurrentDictionary<object, MemoryCacheEntry>();
        _setEntry = SetEntry;
        _entryExpirationNotification = EntryExpired;

        _clock = options.Clock ?? new SystemClock();
        _expirationScanFrequency = options.ExpirationScanFrequency;
        _lastExpirationScan = _clock.UtcNow;
    }
    
    ~MemoryCache()
    {
        Dispose(false);
    }
    
    public int Count => _cacheEntries.Count;

    public ICacheEntry CreateEntry(object key)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(MemoryCache).FullName);
        }
        
        return new MemoryCacheEntry(key, _setEntry, _entryExpirationNotification);
    }

    public bool TryGetValue(object key, out object? value)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(MemoryCache).FullName);
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        value = null;
        var currentTime = _clock.UtcNow;
        
        bool isEntryFound = false;
        if (_cacheEntries.TryGetValue(key, out MemoryCacheEntry? entry))
        {
            if (entry.IsExpired(currentTime) && entry.EvictionReason != EvictionReason.Replaced)
            {
                RemoveEntry(entry);
            }
            else
            {
                isEntryFound = true;
                entry.LastAccessed = currentTime;
                value = entry.Value;
            }
        }

        StartScanForExpiredItems();
        return isEntryFound;
    }

    public void Remove(object key)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(MemoryCache).FullName);
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_cacheEntries.TryRemove(key, out MemoryCacheEntry? cacheEntry))
        {
            cacheEntry.ExpireEntryByReason(EvictionReason.Removed);
            cacheEntry.InvokeEvictionCallbacks();
        }

        StartScanForExpiredItems();
    }

    public void Clear()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(MemoryCache).FullName);
        }

        var keys = _cacheEntries.Keys.ToList();
        foreach (object key in keys)
        {
            if (_cacheEntries.TryRemove(key, out MemoryCacheEntry? cacheEntry))
            {
                cacheEntry.ExpireEntryByReason(EvictionReason.Removed);
                cacheEntry.InvokeEvictionCallbacks();
            }
        }

        StartScanForExpiredItems();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void SetEntry(MemoryCacheEntry entry)
    {
        if (_isDisposed)
        {
            return;
        }

        var currentTime = _clock.UtcNow;
        var entryAbsoluteExpiration = new DateTimeOffset?();
        
        if (entry.AbsoluteExpirationRelativeToNow.HasValue)
        {
            entryAbsoluteExpiration = currentTime + entry.AbsoluteExpirationRelativeToNow;
        }
        else if (entry.AbsoluteExpiration.HasValue)
        {
            entryAbsoluteExpiration = entry.AbsoluteExpiration;
        }

        if (entryAbsoluteExpiration.HasValue &&
            (!entry.AbsoluteExpiration.HasValue || entryAbsoluteExpiration.Value < entry.AbsoluteExpiration.Value))
        {
            entry.AbsoluteExpiration = entryAbsoluteExpiration;
        }

        entry.LastAccessed = currentTime;
        
        if (_cacheEntries.TryGetValue(entry.Key, out MemoryCacheEntry? cacheEntry))
        {
            cacheEntry.ExpireEntryByReason(EvictionReason.Replaced);
        }

        if (!entry.IsExpired(currentTime))
        {
            bool isEntryAdded;
            if (cacheEntry == null)
            {
                isEntryAdded = _cacheEntries.TryAdd(entry.Key, entry);
            }
            else
            {
                isEntryAdded = _cacheEntries.TryUpdate(entry.Key, entry, cacheEntry);
                if (!isEntryAdded)
                {
                    isEntryAdded = _cacheEntries.TryAdd(entry.Key, entry);
                }
            }

            if (isEntryAdded)
            {
                entry.AttachTokens();
            }
            else
            {
                entry.ExpireEntryByReason(EvictionReason.Replaced);
                entry.InvokeEvictionCallbacks();
            }

            cacheEntry?.InvokeEvictionCallbacks();
        }
        else
        {
            entry.InvokeEvictionCallbacks();
            if (cacheEntry != null)
            {
                RemoveEntry(cacheEntry);
            }
        }

        StartScanForExpiredItems();
    }

    private void RemoveEntry(MemoryCacheEntry entry)
    {
        if (!CacheEntries.Remove(new KeyValuePair<object, MemoryCacheEntry>(entry.Key, entry)))
        {
            return;
        }

        entry.InvokeEvictionCallbacks();
    }

    private void EntryExpired(MemoryCacheEntry entry)
    {
        RemoveEntry(entry);
        StartScanForExpiredItems();
    }

    private void StartScanForExpiredItems()
    {
        var currentTime = _clock.UtcNow;
        if (!(_expirationScanFrequency < currentTime - _lastExpirationScan))
        {
            return;
        }

        _lastExpirationScan = currentTime;

        Task.Factory.StartNew(state => ScanForExpiredItems((MemoryCache)state!), this, CancellationToken.None,
            TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    private static void ScanForExpiredItems(MemoryCache cache)
    {
        var currentTime = cache._clock.UtcNow;
        foreach (MemoryCacheEntry entry in cache._cacheEntries.Values)
        {
            if (entry.IsExpired(currentTime))
            {
                cache.RemoveEntry(entry);
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }

        _isDisposed = true;
    }
}