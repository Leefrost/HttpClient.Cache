using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.InMemory;

namespace HttpClient.Cache.Tests.InMemory;

public class MemoryCacheEntryExtensionsTests
{
    [Fact]
    public void SetOptions_SetOptionsToMemoryCacheEntry_OptionsAreSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());
        TestChangeToken changeToken = new("cache-entry");

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetOptions(new MemoryCacheEntryOptions
        {
            Priority = CacheEntryPriority.High,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
            AbsoluteExpiration = DateTimeOffset.Now.AddDays(2),
            SlidingExpiration = TimeSpan.FromMinutes(1),
            ExpirationTokens = { changeToken },
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration((key, value, reason, state) => { }, null)
            }
        });

        using (new AssertionScope())
        {
            entry.AbsoluteExpiration.Should().BeCloseTo(DateTimeOffset.Now.AddDays(2), TimeSpan.FromSeconds(1));
            entry.AbsoluteExpirationRelativeToNow.Should().BeCloseTo(TimeSpan.FromDays(1), TimeSpan.FromSeconds(1));
            entry.SlidingExpiration.Should().BeCloseTo(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(1));
            entry.Priority.Should().Be(CacheEntryPriority.High);
            entry.ExpirationTokens.Should().ContainSingle(item => item.Equals(changeToken));
            entry.PostEvictionCallbacks.Should().HaveCount(1);
        }
    }

    private class TestChangeToken : IChangeToken
    {
        private readonly string _resource;

        public TestChangeToken(string resource)
        {
            _resource = resource;
        }

        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return new DisposableResource();
        }
    }

    private class DisposableResource : IDisposable
    {
        public void Dispose()
        {
        }
    }
}