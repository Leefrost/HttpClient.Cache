﻿using System.Net;
using HttpClient.Cache.Stats;
using HttpClient.Cache.Utils;

namespace HttpClient.Cache.InMemory;

public class InMemoryCacheHandler : DelegatingHandler
{
    private readonly IMemoryCache _responseCache;
    private readonly IDictionary<HttpStatusCode, TimeSpan> _cacheExpirationPerHttpResponseCode;

    public InMemoryCacheHandler(
        HttpMessageHandler? innerHandler,
        IDictionary<HttpStatusCode, TimeSpan>? cacheExpirationPerHttpResponseCode = null,
        ICacheStatsProvider? statsProvider = null,
        ICacheKeysProvider? cacheKeysProvider = null)
            :this(innerHandler, 
                cacheExpirationPerHttpResponseCode, 
                statsProvider, 
                new MemoryCache(new MemoryCacheOptions()), 
                cacheKeysProvider)
    { }
    
    internal InMemoryCacheHandler(
        HttpMessageHandler? innerHandler,
        IDictionary<HttpStatusCode, TimeSpan>? cacheExpirationPerHttpResponseCode,
        ICacheStatsProvider? statsProvider,
        IMemoryCache? responseCache,
        ICacheKeysProvider? cacheKeysProvider)
        : base(innerHandler ?? new HttpClientHandler())
    {
        StatsProvider = statsProvider ?? new DefaultCacheStatsProvider(nameof(InMemoryCacheHandler));
        CacheKeysProvider = cacheKeysProvider ?? new DefaultCacheKeysProvider();

        _cacheExpirationPerHttpResponseCode =
            cacheExpirationPerHttpResponseCode ?? new Dictionary<HttpStatusCode, TimeSpan>();

        _responseCache = responseCache ?? new MemoryCache();
    }
    
    public ICacheKeysProvider CacheKeysProvider { get; }
    
    public ICacheStatsProvider StatsProvider { get; }

    public void InvalidateCache(Uri uri, HttpMethod? method = null)
    {
        var methods = method != null
            ? new[] { method }
            : new[] { HttpMethod.Get, HttpMethod.Head, };

        foreach (var actionMethod in methods)
        {
            var request = new HttpRequestMessage(actionMethod, uri);
            var key = CacheKeysProvider.GetKey(request);
            _responseCache.Remove(key);
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var key = CacheKeysProvider.GetKey(request);
        if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head)
        {
            if (await _responseCache.TryGetAsync(key, out var cachedData) && cachedData != default)
            {
                var cachedResponse = request.RestoreResponseFromCache(cachedData);
                StatsProvider.ReportHit(cachedResponse.StatusCode);

                return cachedResponse;
            };
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head)
        {
            var absoluteExpirationRelativeToNow =
                response.StatusCode.GetAbsoluteExpirationRelativeToNow(_cacheExpirationPerHttpResponseCode);
            
            StatsProvider.ReportMiss(response.StatusCode);

            if (TimeSpan.Zero != absoluteExpirationRelativeToNow)
            {
                var entry = await response.ToCacheDataAsync();
                await _responseCache.TrySetAsync(key, entry, absoluteExpirationRelativeToNow);
                return request.RestoreResponseFromCache(entry);
            }
        }

        return response;
    }

}