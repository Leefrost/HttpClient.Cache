namespace HttpClient.Cache;

public class PostEvictionCallbackRegistration
{
    //TODO: Replace with constructor - callback is not exists without state
    public PostEvictionDelegate? EvictionCallback { get; set; }
    
    public object? State { get; set; }
}

public delegate void PostEvictionDelegate(object key, object? value, string reason, object? state);