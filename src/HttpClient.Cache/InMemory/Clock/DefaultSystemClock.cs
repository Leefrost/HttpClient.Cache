namespace HttpClient.Cache.InMemory.Clock;

/// <summary>
/// Presents current UTC time
/// </summary>
internal class DefaultSystemClock: ISystemClock
{
    /// <summary>
    /// Current UTC now offset.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}