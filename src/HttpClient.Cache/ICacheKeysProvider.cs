namespace HttpClient.Cache;

/// <summary>
/// Cache keys generator
/// </summary>
public interface ICacheKeysProvider
{
    /// <summary>
    /// Generate a cache key for <see cref="HttpRequestMessage"/> http request
    /// </summary>
    /// <param name="request">Http request</param>
    /// <returns>Cache key</returns>
    string GetKey(HttpRequestMessage request);
}