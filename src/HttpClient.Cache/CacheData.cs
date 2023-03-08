namespace HttpClient.Cache;

public class CacheData
{
    public CacheData(byte[] data, HttpResponseMessage response)
    {
        Data = data;
        Response = response;
    }

    public byte[] Data { get; }

    public HttpResponseMessage Response { get; }
}