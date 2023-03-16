using FluentAssertions;

namespace HttpClient.Cache.Tests;

public class DefaultCacheKeysProviderTests
{
    [Fact]
    public void GetKey_GetKeyForMessage_ReturnKey()
    {
        var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri("http://testurl") };
        var cacheProvider = new DefaultCacheKeysProvider();

        var cacheKey = cacheProvider.GetKey(request);

        cacheKey.Should().Be($"MET_{request.Method};URI_{request.RequestUri}");
    }
    
    [Fact]
    public void GetKey_RequestIsNull_ThrowException()
    {
        var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri("http://testurl") };
        var cacheProvider = new DefaultCacheKeysProvider();

        var action = () => cacheProvider.GetKey(null);

        action.Should().Throw<ArgumentNullException>();
    }
}