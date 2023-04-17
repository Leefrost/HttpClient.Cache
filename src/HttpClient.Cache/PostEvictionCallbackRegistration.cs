namespace HttpClient.Cache;

public record PostEvictionCallbackRegistration(PostEvictionDelegate? EvictionCallback, object? State);

public delegate void PostEvictionDelegate(object key, object? value, string reason, object? state);