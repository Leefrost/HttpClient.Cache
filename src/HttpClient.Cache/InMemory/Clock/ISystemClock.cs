namespace HttpClient.Cache.InMemory.Clock;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}