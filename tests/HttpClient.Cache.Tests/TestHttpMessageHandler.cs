using System.Net;
using System.Text;

namespace HttpClient.Cache.Tests;

internal class TestHttpMessageHandler : HttpMessageHandler
{
    private const HttpStatusCode DefaultCode = HttpStatusCode.OK;
    private const string DefaultContent = "The response content";
    private const string DefaultContentType = "text/plain";
    
    private readonly string _content;
    private readonly string _contentType;
    private readonly TimeSpan _delay;
    private readonly Encoding? _encoding;

    private readonly HttpStatusCode _responseCode;

    public TestHttpMessageHandler(
        HttpStatusCode responseCode = DefaultCode,
        string content = DefaultContent,
        string contentType = DefaultContentType,
        Encoding? encoding = null,
        TimeSpan delay = default
    )
    {
        _responseCode = responseCode;
        _content = content;
        _contentType = contentType;
        _encoding = encoding ?? Encoding.UTF8;
        _delay = delay;
    }

    public int CallsMade { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallsMade++;
        if (_delay != default)
        {
            await Task.Delay(_delay, cancellationToken);
        }

        return new HttpResponseMessage
        {
            Content = new StringContent(_content, _encoding, _contentType), 
            StatusCode = _responseCode
        };
    }
}