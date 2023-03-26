using System.Net;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.Utils;

namespace HttpClient.Cache.Tests.Utils;

public class HttpResponseMessageExtensionsTests
{
    [Fact]
    public async Task ToCacheEntry_ConvertResponseToCacheData_ReturnCacheData()
    {
        var content = Encoding.UTF8.GetBytes("The message");
        var response = new HttpResponseMessage
        {
            Content = new ByteArrayContent(content),
            ReasonPhrase = "Reason",
            StatusCode = HttpStatusCode.Found,
            Version = Version.Parse("1.0")
        };

        var cacheData = await response.ToCacheDataAsync();

        cacheData.Data.Should().BeEquivalentTo(content);
        cacheData.Response.Should().NotBeNull().And.NotBe(response);
    }

    [Fact]
    public void RestoreResponseFromCache_RestoreHttpResponse_ReturnHttpResponseMessage()
    {
        var content = Encoding.UTF8.GetBytes("The message");
        var cachedResponse = new HttpResponseMessage
        {
            Content = new ByteArrayContent(content),
            ReasonPhrase = "Reason",
            StatusCode = HttpStatusCode.Found,
            Version = Version.Parse("1.0")
        };
        var headers = new Dictionary<string, IEnumerable<string>> { { "header", new[] { "header-value" } } };
        var contentHeaders = new Dictionary<string, IEnumerable<string>> { { "contentHeader", new[] { "contentHeader-value" } } };
        var cacheData = new CacheData(content, headers, contentHeaders,  cachedResponse);
        var newRequest = new HttpRequestMessage { Method = HttpMethod.Get };

        var restoredResponse = newRequest.RestoreResponseFromCache(cacheData);

        using (new AssertionScope())
        {
            restoredResponse.Should().NotBeNull();
            restoredResponse.Content.Should().NotBeNull();
            restoredResponse.RequestMessage.Should().Be(newRequest);
        }
    }
}