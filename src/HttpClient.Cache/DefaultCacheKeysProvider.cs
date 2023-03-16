namespace HttpClient.Cache;

public class DefaultCacheKeysProvider : ICacheKeysProvider
{
    public string GetKey(HttpRequestMessage? request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return $"MET_{request.Method};URI_{request.RequestUri}";
    }
}