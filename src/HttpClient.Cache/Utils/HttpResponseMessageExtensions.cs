namespace HttpClient.Cache.Utils;

public static class HttpResponseMessageExtensions
{
    public static async Task<CacheData> ToCacheEntry(this HttpResponseMessage response)
    {
        var data = await response.Content.ReadAsByteArrayAsync();
        var copy = new HttpResponseMessage
        {
            ReasonPhrase = response.ReasonPhrase, StatusCode = response.StatusCode, Version = response.Version
        };

        //TODO: headers are important. Will be added later;

        var entry = new CacheData(data, copy);
        return entry;
    }

    public static HttpResponseMessage PrepareCacheEntry(this HttpRequestMessage request, CacheData cacheData)
    {
        var response = cacheData.Response;
        response.Content = new ByteArrayContent(cacheData.Data);
        response.RequestMessage = request;

        //TODO: headers are important. Will be added later;
        
        return response;
    }
}