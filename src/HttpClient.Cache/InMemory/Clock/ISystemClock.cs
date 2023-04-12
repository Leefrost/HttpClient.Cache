namespace HttpClient.Cache.InMemory.Clock;

/// <summary>
/// Defines cache clock
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Current time
    /// </summary>
    DateTimeOffset UtcNow { get; }
}