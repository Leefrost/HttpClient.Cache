namespace HttpClient.Cache;

public class PostEvictionCallbackRegistration
{
    public PostEvictionDelegate EvictionCallback { get; set; }
    
    public object State { get; set; }
}

public delegate void PostEvictionDelegate(object key, object value, string reason, object state);