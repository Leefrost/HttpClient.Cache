namespace HttpClient.Cache.InMemory.Clock;

internal class SystemClock: ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}