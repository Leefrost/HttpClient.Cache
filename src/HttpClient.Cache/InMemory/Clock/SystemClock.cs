namespace HttpClient.Cache.InMemory.Clock;

public class SystemClock: ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}