namespace HttpClient.Cache.Utils;

public static class HttpResponseMessageExtensions
{
    public static async Task<CacheData> ToCacheDataAsync(this HttpResponseMessage response)
    {
        var data = await response.Content.ReadAsByteArrayAsync();
        var copiedResponse = new HttpResponseMessage
        {
            ReasonPhrase = response.ReasonPhrase, StatusCode = response.StatusCode, Version = response.Version
        };
        
        var headers = response.Headers
            .Where(headers => headers.Value.Any())
            .ToDictionary(header => header.Key, header => header.Value);
        
        var contentHeaders = response.Content.Headers
            .Where(contentHeader => contentHeader.Value.Any())
            .ToDictionary(header => header.Key, header => header.Value);

        var entry = new CacheData(data, headers, contentHeaders, copiedResponse);
        return entry;
    }

    public static HttpResponseMessage RestoreResponseFromCache(this HttpRequestMessage request, CacheData cacheData)
    {
        var response = cacheData.Response;
        response.Content = new ByteArrayContent(cacheData.Data);
        response.RequestMessage = request;

        foreach (var kvp in cacheData.Headers)
        {
            response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }

        foreach (var kvp in cacheData.ContentHeaders)
        {
            response.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }

        return response;
    }
}