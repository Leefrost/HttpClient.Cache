namespace HttpClient.Cache;

public class CacheData
{
    public CacheData(
        byte[] data, 
        Dictionary<string, IEnumerable<string>> headers, 
        Dictionary<string, IEnumerable<string>> contentHeaders, 
        HttpResponseMessage response)
    {
        Data = data;
        Response = response;
        Headers = headers;
        ContentHeaders = contentHeaders;
    }
    
    public Dictionary<string, IEnumerable<string>> Headers { get; }
    
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; }

    public byte[] Data { get; }

    public HttpResponseMessage Response { get; }
}