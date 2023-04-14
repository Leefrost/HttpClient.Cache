namespace HttpClient.Cache.InMemory;

/// <summary>
/// Describes the API for InMemory cache
/// </summary>
public interface IMemoryCache: IDisposable
{
    /// <summary>
    /// Getting the cache value by key
    /// </summary>
    /// <param name="key">Entry key</param>
    /// <param name="value">Cache value</param>
    /// <returns>True if entry exist and returned, otherwise - false</returns>
    bool TryGetValue(object key, out object? value);

    /// <summary>
    /// Create empty cache entry
    /// </summary>
    /// <param name="key">Entry key</param>
    /// <returns><see cref="ICacheEntry"/> cache entry</returns>
    ICacheEntry CreateEntry(object key);

    /// <summary>
    /// Remove entry by key
    /// </summary>
    /// <param name="key">Entry key</param>
    void Remove(object key);

    /// <summary>
    /// Clean the cache
    /// </summary>
    void Clear();
}