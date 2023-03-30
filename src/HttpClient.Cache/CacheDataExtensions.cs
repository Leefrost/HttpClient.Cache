using System.Diagnostics;
using Newtonsoft.Json;

namespace HttpClient.Cache;

internal static class CacheDataExtensions
{
    internal static byte[] Pack(this CacheData cacheData)
    {
        var json =  JsonConvert.SerializeObject(cacheData);
        var bytes = new byte[json.Length * sizeof(char)];

        Buffer.BlockCopy(json.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    internal static CacheData? Unpack(this byte[] cacheData)
    {
        try
        {
            var chars = new char[cacheData.Length / sizeof(char)];
            Buffer.BlockCopy(cacheData, 0, chars, 0, cacheData.Length);
            var json = new string(chars);
            
            var data = JsonConvert.DeserializeObject<CacheData>(json);
            return data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            return null;
        }
    }
}