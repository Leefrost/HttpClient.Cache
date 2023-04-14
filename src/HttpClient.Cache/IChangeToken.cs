namespace HttpClient.Cache;

/// <summary>
/// Defines the mechanism for checking the changes over the cache entry
/// </summary>
public interface IChangeToken
{
    /// <summary>
    /// Cache entry is changed
    /// </summary>
    bool HasChanged { get; }
    
    /// <summary>
    /// Call the callback on change
    /// </summary>
    bool ActiveChangeCallbacks { get; }

    /// <summary>
    /// Register a callback to call on change
    /// </summary>
    /// <param name="callback">Callback to call</param>
    /// <param name="state">Current state</param>
    /// <returns><see cref="IDisposable"/> callback handler</returns>
    IDisposable RegisterChangeCallback(Action<object> callback, object state);
}