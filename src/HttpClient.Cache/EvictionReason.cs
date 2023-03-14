namespace HttpClient.Cache;

public enum EvictionReason
{
    None,
    Removed,
    Replaced,
    Expired,
    TokenExpired,
    Capacity
}