using FluentAssertions;
using FluentAssertions.Execution;
using HttpClient.Cache.InMemory;

namespace HttpClient.Cache.Tests.InMemory;

public class MemoryCacheEntryExtensionsTests
{
    [Fact]
    public void SetValue_SetValueToMemoryCacheEntry_ValueIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetValue("the-value");

        entry.Value.Should().Be("the-value");
    }
    
    [Fact]
    public void SetPriority_SetPriorityToMemoryCacheEntry_PriorityIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetPriority(CacheEntryPriority.NeverRemove);

        entry.Priority.Should().Be(CacheEntryPriority.NeverRemove);
    }
    
    [Fact]
    public void SetAbsoluteExpiration_SetAbsoluteExpirationToMemoryCacheEntry_AbsoluteExpirationIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetAbsoluteExpiration(DateTimeOffset.Now.AddDays(1));

        entry.AbsoluteExpiration.Should().BeCloseTo(DateTimeOffset.Now.AddDays(1), TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void SetAbsoluteExpiration_SetAbsoluteExpirationRelativeToNowToMemoryCacheEntry_AbsoluteExpirationIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetAbsoluteExpiration(TimeSpan.FromDays(1));

        entry.AbsoluteExpirationRelativeToNow.Should().BeCloseTo(TimeSpan.FromDays(1), TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void SetSlidingExpiration_SetSlidingExpirationToMemoryCacheEntry_SlidingExpirationIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.SetSlidingExpiration(TimeSpan.FromDays(1));

        entry.SlidingExpiration.Should().BeCloseTo(TimeSpan.FromDays(1), TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void AddExpirationToken_AddExpirationTokenToMemoryCacheEntry_TokenIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());
        TestChangeToken changeToken = new("cache-entry");

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.AddExpirationToken(changeToken);

        entry.ExpirationTokens.Should().ContainSingle(item => item.Equals(changeToken));
    }
    
    [Fact]
    public void AddExpirationToken_AddNullToken_ThrowArgumentException()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        var action = () => entry.AddExpirationToken(null);

        action.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void RegisterPostEvictionCallback_RegisterPostEvictionCallbackToMemoryCacheEntry_CallbackIsSet()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());
        TestChangeToken changeToken = new("cache-entry");

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        entry.RegisterPostEvictionCallback((key, value, reason, state) => { }, null);

        entry.PostEvictionCallbacks.Should().HaveCount(1);
    }
    
    [Fact]
    public void RegisterPostEvictionCallback_AddNullCallback_ThrowArgumentException()
    {
        const string cacheKey = "key";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        using ICacheEntry entry = memoryCache.CreateEntry(cacheKey);
        var action = () => entry.RegisterPostEvictionCallback(null, null);

        action.Should().Throw<ArgumentNullException>();
    }
    
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