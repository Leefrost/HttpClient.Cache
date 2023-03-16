using System.Net;
using FluentAssertions;
using HttpClient.Cache.Utils;

namespace HttpClient.Cache.Tests.Utils;

public class HttpStatusCodeExtensionsTests
{
    private readonly Dictionary<HttpStatusCode, TimeSpan> _cachingTime = new()
    {
        { HttpStatusCode.OK, TimeSpan.FromMinutes(1) }, { HttpStatusCode.BadRequest, TimeSpan.FromSeconds(30) }
    };

    [Fact]
    public void GetAbsoluteExpirationRelativeToNow_ConvertExistedCodeToTimeSpan_Return1min()
    {
        const HttpStatusCode existingCode = HttpStatusCode.OK;

        var relativeCacheTime = existingCode.GetAbsoluteExpirationRelativeToNow(_cachingTime);

        relativeCacheTime.Should().Be(TimeSpan.FromMinutes(1));
    }
    
    [Fact]
    public void GetAbsoluteExpirationRelativeToNow_ConvertCloseCodeToParentCode_Return30s()
    {
        const HttpStatusCode existingCode = HttpStatusCode.Gone;

        var relativeCacheTime = existingCode.GetAbsoluteExpirationRelativeToNow(_cachingTime);

        relativeCacheTime.Should().Be(TimeSpan.FromSeconds(30));
    }
    
    [Fact]
    public void GetAbsoluteExpirationRelativeToNow_GetDefaultCacheIfCodeUnknown_Return1Day()
    {
        const HttpStatusCode existingCode = HttpStatusCode.InternalServerError;

        var relativeCacheTime = existingCode.GetAbsoluteExpirationRelativeToNow(_cachingTime);

        relativeCacheTime.Should().Be(TimeSpan.FromDays(1));
    }
}