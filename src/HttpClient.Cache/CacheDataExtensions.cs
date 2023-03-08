using System.Text.Json;

namespace HttpClient.Cache;

public static class CacheDataExtensions
{
    public static byte[] Serialize(this CacheData cacheData)
    {
        string json = JsonSerializer.Serialize(cacheData);
        byte[] bytes = new byte[json.Length * sizeof(char)];

        Buffer.BlockCopy(json.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static CacheData? Deserialize(this byte[] cacheData)
    {
        try
        {
            char[] chars = new char[cacheData.Length / sizeof(char)];
            Buffer.BlockCopy(cacheData, 0, chars, 0, cacheData.Length);
            string json = new string(chars);
            CacheData? data = JsonSerializer.Deserialize<CacheData>(json);
            return data;
        }
        catch
        {
            return null;
        }
    }
}