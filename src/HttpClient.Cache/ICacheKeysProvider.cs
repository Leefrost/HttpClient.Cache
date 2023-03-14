namespace HttpClient.Cache;

public interface ICacheKeysProvider
{
    string GetKey(HttpRequestMessage request);
}