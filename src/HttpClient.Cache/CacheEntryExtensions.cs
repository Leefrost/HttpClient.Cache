namespace HttpClient.Cache;

internal static class CacheEntryExtensions
{
    internal static ICacheEntry AddExpirationToken(this ICacheEntry entry, IChangeToken token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }
        
        entry.ExpirationTokens.Add(token);
        return entry;
    }
}