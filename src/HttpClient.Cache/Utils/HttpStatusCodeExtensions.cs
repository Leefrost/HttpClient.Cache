using System.Net;

namespace HttpClient.Cache.Utils;

public static class HttpStatusCodeExtensions
{
    public static TimeSpan GetAbsoluteExpirationRelativeToNow(this HttpStatusCode statusCode,
        IDictionary<HttpStatusCode, TimeSpan> mapping)
    {
        if (mapping.TryGetValue(statusCode, out var expiration))
        {
            return expiration;
        }
        
        var code = (int)statusCode;
        if (mapping.TryGetValue((HttpStatusCode)(Math.Floor(code / 100.0) * 100), out expiration))
        {
            return expiration;
        }

        return TimeSpan.FromDays(1);
    }
}