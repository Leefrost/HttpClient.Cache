using FluentAssertions;
using HttpClient.Cache.InMemory;

namespace HttpClient.Cache.Tests.InMemory;

public class InMemoryCacheHandlerTests
{
    [Fact]
    public async Task Cache_HandleNewCacheMessage_Successful()
    {
        var testHandler = new TestHandler();
        var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testHandler));

        await client.GetAsync("http://the-url");
        await client.GetAsync("http://the-url");

        testHandler.CallsMade.Should().Be(1);
    }
}